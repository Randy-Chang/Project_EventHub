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

    private static bool IsConstraintViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException { SqliteErrorCode: 19 };
}
