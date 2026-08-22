using EventHub.Application.Quizzes;
using EventHub.Domain.Quizzes;
using EventHub.Infrastructure.Persistence;
using EventHub.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Tests;

public sealed class QuestionBankPersistenceTests
{
    [Fact]
    public async Task Import_PersistsQuizQuestionsOptionsAndMetadataAcrossRestart()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"eventhub-question-bank-{Guid.NewGuid():N}.db");
        var options = CreateOptions(databasePath);
        var eventId = Guid.NewGuid();
        var quiz = Quiz.Create(eventId, "尾牙題庫", DateTimeOffset.UtcNow);
        var question = CreateQuestion(quiz.Id, "Q1", 1);
        try
        {
            await using (var context = new EventHubDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();
                context.Events.Add(CreateEvent(eventId));
                await context.SaveChangesAsync();
                await new QuestionBankRepository(context).ImportAsync(quiz, [question], CancellationToken.None);
            }

            await using (var restarted = new EventHubDbContext(options))
            {
                var stored = await restarted.QuizQuestions.Include(item => item.Options).SingleAsync();
                Assert.Equal("Q1", stored.QuestionKey);
                Assert.Equal("公司", stored.Category);
                Assert.Equal(QuizQuestionDifficulty.Medium, stored.Difficulty);
                Assert.Equal(4, stored.Options.Count);
                Assert.Equal(stored.Options.OrderBy(option => option.Order).ElementAt(1).Id, stored.CorrectOptionId);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    [Fact]
    public async Task Import_WhenAnyQuestionConflicts_RollsBackEverything()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"eventhub-question-bank-{Guid.NewGuid():N}.db");
        var options = CreateOptions(databasePath);
        var eventId = Guid.NewGuid();
        try
        {
            await using var context = new EventHubDbContext(options);
            await context.Database.EnsureCreatedAsync();
            context.Events.Add(CreateEvent(eventId));
            await context.SaveChangesAsync();
            var quiz = Quiz.Create(eventId, "Rollback", DateTimeOffset.UtcNow);
            var repository = new QuestionBankRepository(context);

            await Assert.ThrowsAsync<QuestionBankImportConflictException>(() => repository.ImportAsync(
                quiz,
                [CreateQuestion(quiz.Id, "Q1", 1), CreateQuestion(quiz.Id, "Q2", 1)],
                CancellationToken.None));

            Assert.Empty(await context.Quizzes.AsNoTracking().ToListAsync());
            Assert.Empty(await context.QuizQuestions.AsNoTracking().ToListAsync());
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    [Fact]
    public async Task QuizTitle_IsUniqueWithinEventCaseInsensitive()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"eventhub-question-bank-{Guid.NewGuid():N}.db");
        var options = CreateOptions(databasePath);
        var eventId = Guid.NewGuid();
        try
        {
            await using var context = new EventHubDbContext(options);
            await context.Database.EnsureCreatedAsync();
            context.Events.Add(CreateEvent(eventId));
            context.Quizzes.Add(Quiz.Create(eventId, "Annual Quiz", DateTimeOffset.UtcNow));
            await context.SaveChangesAsync();
            context.Quizzes.Add(Quiz.Create(eventId, "annual quiz", DateTimeOffset.UtcNow));
            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    private static DbContextOptions<EventHubDbContext> CreateOptions(string path) =>
        new DbContextOptionsBuilder<EventHubDbContext>()
            .UseSqlite($"Data Source={path};Foreign Keys=True")
            .Options;

    private static EventHub.Domain.Events.Event CreateEvent(Guid id)
    {
        var item = EventHub.Domain.Events.Event.Create(
            "Event",
            "ABC234",
            DateTimeOffset.UtcNow,
            "host-hash",
            DateTimeOffset.UtcNow);
        typeof(EventHub.Domain.Events.Event).GetProperty(nameof(item.Id))!.SetValue(item, id);
        return item;
    }

    private static QuizQuestion CreateQuestion(Guid quizId, string key, int order) =>
        QuizQuestion.Create(
            quizId,
            key,
            "公司",
            QuizQuestionDifficulty.Medium,
            "題目",
            ["A", "B", "C", "D"],
            1,
            TimeSpan.FromSeconds(20),
            order);
}
