using EventHub.Application.Abstractions;
using EventHub.Application.Quizzes;
using EventHub.Domain.Quizzes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence.Repositories;

public sealed class QuestionBankRepository(EventHubDbContext dbContext) : IQuestionBankRepository
{
    public Task<bool> TitleExistsAsync(Guid eventId, string title, CancellationToken cancellationToken) =>
        dbContext.Quizzes.AsNoTracking().AnyAsync(
            quiz => quiz.EventId == eventId && EF.Functions.Collate(quiz.Title, "NOCASE") == title,
            cancellationToken);

    public async Task<IReadOnlyList<QuestionBankData>> ListAsync(
        Guid eventId,
        CancellationToken cancellationToken) =>
        await dbContext.Quizzes.AsNoTracking()
            .Where(quiz => quiz.EventId == eventId)
            .OrderBy(quiz => quiz.CreatedAtUtc)
            .Select(quiz => new QuestionBankData(
                quiz.Id,
                quiz.Title,
                dbContext.QuizQuestions.Count(question => question.QuizId == quiz.Id),
                quiz.CreatedAtUtc))
            .ToListAsync(cancellationToken);

    public Task<QuestionBankData?> GetAsync(
        Guid eventId,
        Guid quizId,
        CancellationToken cancellationToken) =>
        dbContext.Quizzes.AsNoTracking()
            .Where(quiz => quiz.EventId == eventId && quiz.Id == quizId)
            .Select(quiz => new QuestionBankData(
                quiz.Id,
                quiz.Title,
                dbContext.QuizQuestions.Count(question => question.QuizId == quiz.Id),
                quiz.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<QuestionBankQuestionData>> ListQuestionsAsync(
        Guid eventId,
        Guid quizId,
        CancellationToken cancellationToken)
    {
        var currentSession = await dbContext.QuizQuestionSessions.AsNoTracking()
            .Where(session => session.EventId == eventId)
            .OrderByDescending(session => session.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        var revealedQuestionIds = await dbContext.QuizQuestionSessions.AsNoTracking()
            .Where(session => session.EventId == eventId &&
                session.QuizId == quizId &&
                session.State == QuizQuestionState.Revealed)
            .Select(session => session.QuestionId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var questions = await dbContext.QuizQuestions.AsNoTracking()
            .Include(question => question.Options)
            .Where(question => question.QuizId == quizId)
            .OrderBy(question => question.Order)
            .ToListAsync(cancellationToken);
        return questions.Select(question => new QuestionBankQuestionData(
            question,
            currentSession?.QuestionId == question.Id,
            revealedQuestionIds.Contains(question.Id))).ToArray();
    }

    public async Task ImportAsync(
        Quiz quiz,
        IReadOnlyCollection<QuizQuestion> questions,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            dbContext.Quizzes.Add(quiz);
            dbContext.QuizQuestions.AddRange(questions);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqliteException { SqliteErrorCode: 19 })
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            throw new QuestionBankImportConflictException("題庫名稱、QuestionKey 或 Order 與現有資料重複。");
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            throw;
        }
    }
}
