using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Application.Quizzes;
using EventHub.Domain.Quizzes;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Application.Tests;

public sealed class QuestionBankServiceTests
{
    [Fact]
    public async Task Preview_ValidCsv_DoesNotWriteData()
    {
        var fixture = CreateFixture();
        var preview = await fixture.Service.PreviewAsync(
            fixture.EventId,
            "host",
            "quiz.csv",
            Stream.Null,
            100,
            CancellationToken.None);

        Assert.True(preview.IsValid);
        Assert.Equal("正式題庫", preview.QuizTitle);
        Assert.Null(fixture.Repository.ImportedQuiz);
    }

    [Fact]
    public async Task Import_RevalidatesAndMapsAllFields()
    {
        var fixture = CreateFixture();
        _ = await fixture.Service.PreviewAsync(
            fixture.EventId,
            "host",
            "quiz.csv",
            Stream.Null,
            100,
            CancellationToken.None);
        var result = await fixture.Service.ImportAsync(
            fixture.EventId,
            "host",
            "quiz.csv",
            Stream.Null,
            100,
            CancellationToken.None);

        Assert.Equal(1, result.QuestionCount);
        Assert.Equal(2, fixture.Parser.CallCount);
        var question = Assert.Single(fixture.Repository.ImportedQuestions!);
        Assert.Equal("Q1", question.QuestionKey);
        Assert.Equal("公司", question.Category);
        Assert.Equal(QuizQuestionDifficulty.Hard, question.Difficulty);
        Assert.Equal(QuizQuestionMode.Scored, question.Mode);
        Assert.Equal("答案說明", question.Explanation);
        Assert.Equal(4, question.Options.Count);
        Assert.Equal(question.Options.OrderBy(option => option.Order).ElementAt(1).Id, question.CorrectOptionId);
    }

    [Fact]
    public async Task Import_InvalidCsv_DoesNotWriteAnything()
    {
        var fixture = CreateFixture(new QuestionBankRawRow(
            2, "Q1", "正式題庫", "公司", "Hard", "1", "題目", "A", "B", "C", "D", "Z", "20", "", "Scored"));

        var exception = await Assert.ThrowsAsync<QuestionBankImportValidationException>(() => fixture.Service.ImportAsync(
            fixture.EventId,
            "host",
            "quiz.csv",
            Stream.Null,
            100,
            CancellationToken.None));

        Assert.False(exception.Preview.IsValid);
        Assert.Null(fixture.Repository.ImportedQuiz);
    }

    [Fact]
    public async Task Preview_DuplicateQuizTitle_IsRejected()
    {
        var fixture = CreateFixture();
        fixture.Repository.TitleExists = true;
        var preview = await fixture.Service.PreviewAsync(
            fixture.EventId,
            "host",
            "quiz.csv",
            Stream.Null,
            100,
            CancellationToken.None);

        Assert.False(preview.IsValid);
        Assert.Contains(preview.Issues, issue => issue.Field == "QuizTitle");
    }

    [Fact]
    public async Task EnsureDefaultPractice_CreatesSeparatePracticeOnlyQuizAndIsRepeatable()
    {
        var fixture = CreateFixture();

        var first = await fixture.Service.EnsureDefaultPracticeAsync(
            fixture.EventId,
            "host",
            CancellationToken.None);
        var second = await fixture.Service.EnsureDefaultPracticeAsync(
            fixture.EventId,
            "host",
            CancellationToken.None);

        Assert.Equal(first.QuizId, second.QuizId);
        Assert.Equal(DefaultPracticeQuestionProvider.QuizTitle, first.QuizTitle);
        Assert.Equal(2, first.QuestionCount);
        Assert.All(fixture.Repository.ImportedQuestions!, question =>
            Assert.Equal(QuizQuestionMode.Practice, question.Mode));
    }

    private static Fixture CreateFixture(QuestionBankRawRow? row = null)
    {
        var eventItem = DomainEvent.Create(
            "Event",
            "ABC234",
            DateTimeOffset.UtcNow,
            "hash:host",
            DateTimeOffset.UtcNow);
        var eventRepository = new FakeEventRepository(eventItem);
        var eventService = new EventService(
            eventRepository,
            new FakeCredentialService(),
            new FakeJoinCodeGenerator(),
            new EventJoinUrlBuilder("http://localhost:5000"),
            TimeProvider.System);
        var parser = new FakeParser(row ?? new QuestionBankRawRow(
            2, "Q1", "正式題庫", "公司", "Hard", "1", "題目", "A", "B", "C", "D", "B", "20", "答案說明", "Scored"));
        var repository = new FakeQuestionBankRepository();
        var service = new QuestionBankService(
            parser,
            repository,
            eventService,
            new QuestionBankValidator(),
            TimeProvider.System,
            new DefaultPracticeQuestionProvider());
        return new Fixture(eventItem.Id, service, parser, repository);
    }

    private sealed record Fixture(
        Guid EventId,
        QuestionBankService Service,
        FakeParser Parser,
        FakeQuestionBankRepository Repository);

    private sealed class FakeParser(QuestionBankRawRow row) : IQuestionBankCsvParser
    {
        public int CallCount { get; private set; }
        public Task<QuestionBankParseResult> ParseAsync(Stream stream, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(new QuestionBankParseResult([row], []));
        }
    }

    private sealed class FakeQuestionBankRepository : IQuestionBankRepository
    {
        public bool TitleExists { get; set; }
        public Quiz? ImportedQuiz { get; private set; }
        public IReadOnlyCollection<QuizQuestion>? ImportedQuestions { get; private set; }
        public Task<bool> TitleExistsAsync(Guid eventId, string title, CancellationToken cancellationToken) =>
            Task.FromResult(TitleExists);
        public Task<IReadOnlyList<QuestionBankData>> ListAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<QuestionBankData>>(ImportedQuiz is null
                ? []
                : [new QuestionBankData(
                    ImportedQuiz.Id,
                    ImportedQuiz.Title,
                    ImportedQuestions?.Count ?? 0,
                    ImportedQuiz.CreatedAtUtc)]);
        public Task<QuestionBankData?> GetAsync(Guid eventId, Guid quizId, CancellationToken cancellationToken) =>
            Task.FromResult<QuestionBankData?>(null);
        public Task<IReadOnlyList<QuestionBankQuestionData>> ListQuestionsAsync(Guid eventId, Guid quizId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<QuestionBankQuestionData>>([]);
        public Task ImportAsync(Quiz quiz, IReadOnlyCollection<QuizQuestion> questions, CancellationToken cancellationToken)
        {
            ImportedQuiz = quiz;
            ImportedQuestions = questions;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeEventRepository(DomainEvent item) : IEventRepository
    {
        public Task<DomainEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult<DomainEvent?>(item.Id == eventId ? item : null);
        public Task<DomainEvent?> GetByJoinCodeAsync(string normalizedJoinCode, CancellationToken cancellationToken) =>
            Task.FromResult<DomainEvent?>(null);
        public Task<bool> JoinCodeExistsAsync(string normalizedJoinCode, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<bool> TryAddAsync(DomainEvent eventItem, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task UpdateAsync(DomainEvent eventItem, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeCredentialService : ICredentialService
    {
        public string GenerateToken() => "host";
        public string HashToken(string token) => $"hash:{token}";
        public bool Matches(string token, string expectedHash) => HashToken(token) == expectedHash;
    }

    private sealed class FakeJoinCodeGenerator : IEventJoinCodeGenerator
    {
        public string Generate() => "ABC234";
    }
}
