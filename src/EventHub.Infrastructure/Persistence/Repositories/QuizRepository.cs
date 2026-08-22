using EventHub.Application.Abstractions;
using EventHub.Application.Quizzes;
using EventHub.Domain.Quizzes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence.Repositories;

public sealed class QuizRepository(EventHubDbContext dbContext) : IQuizRepository
{
    public Task<Quiz?> GetByEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return dbContext.Quizzes
            .OrderBy(quiz => quiz.CreatedAtUtc)
            .FirstOrDefaultAsync(quiz => quiz.EventId == eventId, cancellationToken);
    }

    public async Task AddQuizAsync(Quiz quiz, CancellationToken cancellationToken)
    {
        dbContext.Quizzes.Add(quiz);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetNextQuestionOrderAsync(Guid quizId, CancellationToken cancellationToken)
    {
        var lastOrder = await dbContext.QuizQuestions
            .Where(question => question.QuizId == quizId)
            .Select(question => (int?)question.Order)
            .MaxAsync(cancellationToken);
        return (lastOrder ?? 0) + 1;
    }

    public Task<int> CountQuestionsAsync(Guid quizId, CancellationToken cancellationToken)
    {
        return dbContext.QuizQuestions.CountAsync(
            question => question.QuizId == quizId,
            cancellationToken);
    }

    public async Task AddQuestionAsync(QuizQuestion question, CancellationToken cancellationToken)
    {
        dbContext.QuizQuestions.Add(question);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<QuizQuestion?> GetQuestionAsync(Guid questionId, CancellationToken cancellationToken)
    {
        return dbContext.QuizQuestions
            .Include(question => question.Options)
            .SingleOrDefaultAsync(question => question.Id == questionId, cancellationToken);
    }

    public Task<QuizQuestionSession?> GetCurrentSessionAsync(
        Guid eventId,
        CancellationToken cancellationToken)
    {
        return dbContext.QuizQuestionSessions
            .Where(session => session.EventId == eventId)
            .OrderByDescending(session => session.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddSessionAsync(QuizQuestionSession session, CancellationToken cancellationToken)
    {
        dbContext.QuizQuestionSessions.Add(session);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            dbContext.Entry(session).State = EntityState.Detached;
            throw new QuizApplicationException(
                QuizErrorCode.QuestionAlreadyOpen,
                "已有題目正在開放作答。");
        }
    }

    public async Task UpdateSessionAsync(QuizQuestionSession session, CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Multiple clients may discover the same expired deadline. The first write wins;
            // reload so every caller continues with the server's persisted final state.
            await dbContext.Entry(session).ReloadAsync(cancellationToken);
        }
    }

    public Task<ParticipantAnswer?> GetAnswerAsync(
        Guid questionSessionId,
        Guid participantId,
        CancellationToken cancellationToken)
    {
        return dbContext.ParticipantAnswers
            .AsNoTracking()
            .SingleOrDefaultAsync(answer =>
                answer.QuestionSessionId == questionSessionId &&
                answer.ParticipantId == participantId,
                cancellationToken);
    }

    public async Task<bool> TryAddAnswerAsync(
        ParticipantAnswer answer,
        CancellationToken cancellationToken)
    {
        dbContext.ParticipantAnswers.Add(answer);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsConstraintViolation(exception))
        {
            dbContext.Entry(answer).State = EntityState.Detached;
            return false;
        }
    }

    public Task<int> CountAnswersAsync(Guid questionSessionId, CancellationToken cancellationToken)
    {
        return dbContext.ParticipantAnswers.CountAsync(
            answer => answer.QuestionSessionId == questionSessionId,
            cancellationToken);
    }

    public async Task<bool> ExecuteRevealAndScoreAsync(
        Guid eventId,
        Guid questionSessionId,
        Func<QuizRevealSnapshot, QuizRevealChanges> calculateChanges,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var session = await dbContext.QuizQuestionSessions.SingleOrDefaultAsync(
                item => item.EventId == eventId && item.Id == questionSessionId,
                cancellationToken)
                ?? throw new QuizApplicationException(QuizErrorCode.SessionNotFound, "找不到指定的題目場次。");
            if (session.State == QuizQuestionState.Revealed)
            {
                return false;
            }

            var question = await dbContext.QuizQuestions
                .Include(item => item.Options)
                .SingleAsync(item => item.Id == session.QuestionId, cancellationToken);
            var answers = await dbContext.ParticipantAnswers
                .Where(answer => answer.QuestionSessionId == questionSessionId)
                .OrderBy(answer => answer.ParticipantId)
                .ToListAsync(cancellationToken);
            var participantIds = answers.Select(answer => answer.ParticipantId).ToArray();
            var scores = await dbContext.ParticipantQuizScores
                .Where(score => score.QuizId == session.QuizId && participantIds.Contains(score.ParticipantId))
                .ToListAsync(cancellationToken);

            var changes = calculateChanges(new QuizRevealSnapshot(session, question, answers, scores));
            dbContext.ParticipantQuestionResults.AddRange(changes.QuestionResults);
            dbContext.ParticipantQuizScores.AddRange(changes.NewParticipantScores);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception exception) when (IsRevealWriteConflict(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            var wasRevealed = await dbContext.QuizQuestionSessions
                .AsNoTracking()
                .AnyAsync(
                    session => session.EventId == eventId &&
                        session.Id == questionSessionId &&
                        session.State == QuizQuestionState.Revealed,
                    cancellationToken);
            if (wasRevealed)
            {
                return false;
            }

            throw;
        }
    }

    public Task<ParticipantQuestionResult?> GetQuestionResultAsync(
        Guid questionSessionId,
        Guid participantId,
        CancellationToken cancellationToken)
    {
        return dbContext.ParticipantQuestionResults
            .AsNoTracking()
            .SingleOrDefaultAsync(
                result => result.QuestionSessionId == questionSessionId && result.ParticipantId == participantId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<QuizLeaderboardRow>> ListLeaderboardRowsAsync(
        Guid eventId,
        Guid quizId,
        CancellationToken cancellationToken)
    {
        return await (
            from participant in dbContext.Participants.AsNoTracking()
            where participant.EventId == eventId
            join score in dbContext.ParticipantQuizScores.AsNoTracking().Where(item => item.QuizId == quizId)
                on participant.Id equals score.ParticipantId into participantScores
            from score in participantScores.DefaultIfEmpty()
            select new QuizLeaderboardRow(
                participant.Id,
                participant.Nickname ?? participant.Name,
                participant.Department,
                participant.TableNumber,
                score == null ? 0 : score.TotalScore,
                score == null ? 0 : score.CorrectCount,
                score == null ? 0 : score.AnsweredCount))
            .ToListAsync(cancellationToken);
    }

    public async Task<QuizSessionStatisticsData?> GetSessionStatisticsAsync(
        Guid eventId,
        Guid questionSessionId,
        CancellationToken cancellationToken)
    {
        var session = await dbContext.QuizQuestionSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.EventId == eventId && item.Id == questionSessionId,
                cancellationToken);
        if (session is null)
        {
            return null;
        }

        var participantCount = await dbContext.Participants.CountAsync(
            participant => participant.EventId == eventId,
            cancellationToken);
        var optionCounts = await dbContext.ParticipantAnswers
            .Where(answer => answer.QuestionSessionId == questionSessionId)
            .GroupBy(answer => answer.SelectedOptionId)
            .Select(group => new QuizOptionAnswerCount(group.Key, group.Count()))
            .ToListAsync(cancellationToken);
        var answeredCount = optionCounts.Sum(item => item.AnswerCount);
        var correctCount = await dbContext.ParticipantQuestionResults.CountAsync(
            result => result.QuestionSessionId == questionSessionId && result.IsCorrect,
            cancellationToken);
        return new QuizSessionStatisticsData(
            session.QuizId,
            session.QuestionId,
            participantCount,
            answeredCount,
            correctCount,
            optionCounts);
    }

    private static bool IsConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteErrorCode: 19 };

    private static bool IsRevealWriteConflict(Exception exception) => exception switch
    {
        DbUpdateConcurrencyException => true,
        DbUpdateException updateException when updateException.InnerException is SqliteException
            { SqliteErrorCode: 5 or 6 or 19 } => true,
        SqliteException { SqliteErrorCode: 5 or 6 or 19 } => true,
        _ => false
    };
}
