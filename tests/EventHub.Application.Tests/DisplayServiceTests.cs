using EventHub.Application.Abstractions;
using EventHub.Application.Display;
using EventHub.Application.Events;
using EventHub.Application.Participants;
using EventHub.Application.Quizzes;
using EventHub.Domain.Common;
using EventHub.Domain.Events;
using EventHub.Domain.Participants;
using EventHub.Domain.Quizzes;
using System.Text.Json;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Application.Tests;

public sealed class DisplayServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 22, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Waiting_ReturnsPhase35JoinInfoAndParticipantCount()
    {
        var fixture = CreateFixture(QuizQuestionState.Waiting);

        var state = await fixture.Service.GetCurrentStateAsync(fixture.EventId, 10, CancellationToken.None);

        Assert.Equal(DisplayMode.Waiting, state.Mode);
        Assert.Equal("ABC234", state.JoinInfo?.JoinCode);
        Assert.Equal("http://192.168.1.20:5000/join/ABC234", state.JoinInfo?.JoinUrl);
        Assert.Equal(3, state.ParticipantCount);
    }

    [Theory]
    [InlineData(QuizQuestionState.Open)]
    [InlineData(QuizQuestionState.Closed)]
    public async Task QuestionBeforeReveal_DoesNotLeakCorrectAnswer(QuizQuestionState quizState)
    {
        var fixture = CreateFixture(quizState);
        await fixture.Service.SetModeAsync(
            new SetDisplayModeCommand(fixture.EventId, DisplayMode.Question, "host-token"),
            10,
            CancellationToken.None);

        var state = await fixture.Service.GetCurrentStateAsync(fixture.EventId, 10, CancellationToken.None);

        Assert.Equal(DisplayMode.Question, state.Mode);
        Assert.Equal(quizState, state.Question?.State);
        Assert.Null(state.Question?.CorrectOptionId);
        Assert.Null(state.Question?.Explanation);
        Assert.DoesNotContain(
            "CorrectOptionId",
            JsonSerializer.Serialize(state),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            "Explanation",
            JsonSerializer.Serialize(state),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Result_AfterRevealReturnsCorrectAnswerAndServerStatistics()
    {
        var fixture = CreateFixture(QuizQuestionState.Revealed);

        var state = await fixture.Service.SetModeAsync(
            new SetDisplayModeCommand(fixture.EventId, DisplayMode.Result, "host-token"),
            10,
            CancellationToken.None);

        Assert.Equal(DisplayMode.Result, state.Mode);
        Assert.Equal(fixture.Question.CorrectOptionId, state.Question?.CorrectOptionId);
        Assert.Equal("2000 是正確年份。", state.Question?.Explanation);
        Assert.Equal(2, state.Statistics?.CorrectCount);
        Assert.Equal(0.6667m, state.Statistics?.CorrectRate);
    }

    [Fact]
    public async Task Leaderboard_ReturnsConfiguredTopUsingExistingRanking()
    {
        var fixture = CreateFixture(QuizQuestionState.Revealed);

        var state = await fixture.Service.SetModeAsync(
            new SetDisplayModeCommand(fixture.EventId, DisplayMode.Leaderboard, "host-token"),
            2,
            CancellationToken.None);

        Assert.Collection(
            state.Leaderboard,
            first =>
            {
                Assert.Equal(1, first.Rank);
                Assert.Equal("Amy", first.DisplayName);
                Assert.Equal(900, first.TotalScore);
            },
            second => Assert.Equal("Ben", second.DisplayName));
    }

    [Fact]
    public async Task Result_BeforeRevealIsRejected()
    {
        var fixture = CreateFixture(QuizQuestionState.Closed);

        await Assert.ThrowsAsync<DomainValidationException>(() => fixture.Service.SetModeAsync(
            new SetDisplayModeCommand(fixture.EventId, DisplayMode.Result, "host-token"),
            10,
            CancellationToken.None));
    }

    [Fact]
    public async Task InvalidEvent_IsNotFound()
    {
        var fixture = CreateFixture(QuizQuestionState.Waiting);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => fixture.Service.GetCurrentStateAsync(
            Guid.NewGuid(),
            10,
            CancellationToken.None));
    }

    [Fact]
    public async Task CurrentStateQuery_RecoversPersistedDisplayMode()
    {
        var fixture = CreateFixture(QuizQuestionState.Revealed);
        await fixture.Service.SetModeAsync(
            new SetDisplayModeCommand(fixture.EventId, DisplayMode.Leaderboard, "host-token"),
            10,
            CancellationToken.None);

        var recovered = await fixture.Service.GetCurrentStateAsync(
            fixture.EventId,
            10,
            CancellationToken.None);

        Assert.Equal(DisplayMode.Leaderboard, recovered.Mode);
    }

    [Fact]
    public async Task QuestionNumber_UsesModeSpecificPosition()
    {
        var fixture = CreateFixture(QuizQuestionState.Open);
        fixture.Repository.Position = new QuizQuestionPosition(2, 30);
        await fixture.Service.SetModeAsync(
            new SetDisplayModeCommand(fixture.EventId, DisplayMode.Question, "host-token"),
            10,
            CancellationToken.None);

        var state = await fixture.Service.GetCurrentStateAsync(fixture.EventId, 10, CancellationToken.None);

        Assert.Equal(2, state.Question?.QuestionNumber);
        Assert.Equal(30, state.Question?.TotalQuestionCount);
    }

    private static Fixture CreateFixture(QuizQuestionState sessionState)
    {
        var credentials = new FakeCredentialService();
        var eventItem = DomainEvent.Create(
            "Annual Party",
            "ABC234",
            Now.AddDays(1),
            credentials.HashToken("host-token"),
            Now);
        var eventRepository = new FakeEventRepository(eventItem);
        var participantRepository = new FakeParticipantRepository(3);
        var presenceStore = new FakePresenceStore();
        var quizRepository = new FakeQuizRepository();
        var quiz = Quiz.Create(eventItem.Id, "Quiz", Now);
        var question = QuizQuestion.Create(
            quiz.Id,
            "Q1",
            "公司",
            QuizQuestionDifficulty.Medium,
            QuizQuestionMode.Scored,
            "公司成立於哪一年？",
            "2000 是正確年份。",
            ["1998", "2000", "2004", "2008"],
            1,
            TimeSpan.FromSeconds(20),
            1);
        quizRepository.Quiz = quiz;
        quizRepository.Question = question;
        if (sessionState != QuizQuestionState.Waiting)
        {
            var session = QuizQuestionSession.Create(eventItem.Id, quiz.Id, question.Id);
            session.Open(Now, question.AnswerDuration);
            if (sessionState >= QuizQuestionState.Closed)
            {
                session.Close(Now.AddSeconds(20));
            }

            if (sessionState == QuizQuestionState.Revealed)
            {
                session.Reveal(Now.AddSeconds(21));
            }

            quizRepository.Session = session;
        }

        quizRepository.LeaderboardRows.AddRange([
            new QuizLeaderboardRow(Guid.NewGuid(), "Cara", null, null, 300, 1, 1),
            new QuizLeaderboardRow(Guid.NewGuid(), "Amy", null, null, 900, 2, 2),
            new QuizLeaderboardRow(Guid.NewGuid(), "Ben", null, null, 600, 1, 2)
        ]);
        var eventService = new EventService(
            eventRepository,
            credentials,
            new FakeJoinCodeGenerator(),
            new EventJoinUrlBuilder("http://192.168.1.20:5000"),
            new FixedTimeProvider(Now));
        var participantService = new ParticipantService(
            participantRepository,
            presenceStore,
            credentials,
            eventService,
            new FixedTimeProvider(Now));
        var scoringService = new QuizScoringService(
            quizRepository,
            eventService,
            participantService,
            new QuizScoreCalculator(),
            new FixedTimeProvider(Now));
        var service = new DisplayService(
            eventRepository,
            participantRepository,
            quizRepository,
            eventService,
            scoringService,
            new FixedTimeProvider(Now));
        return new Fixture(eventItem.Id, question, service, quizRepository);
    }

    private sealed record Fixture(
        Guid EventId,
        QuizQuestion Question,
        DisplayService Service,
        FakeQuizRepository Repository);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeCredentialService : ICredentialService
    {
        public string GenerateToken() => "generated-token-with-at-least-32-characters";
        public string HashToken(string token) => $"hash:{token}";
        public bool Matches(string token, string expectedHash) => HashToken(token) == expectedHash;
    }

    private sealed class FakeJoinCodeGenerator : IEventJoinCodeGenerator
    {
        public string Generate() => "ABC234";
    }

    private sealed class FakeEventRepository(DomainEvent eventItem) : IEventRepository
    {
        public Task<DomainEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult(eventItem.Id == eventId ? eventItem : null);

        public Task<DomainEvent?> GetByJoinCodeAsync(string normalizedJoinCode, CancellationToken cancellationToken) =>
            Task.FromResult(eventItem.JoinCode == normalizedJoinCode ? eventItem : null);

        public Task<bool> JoinCodeExistsAsync(string normalizedJoinCode, CancellationToken cancellationToken) =>
            Task.FromResult(eventItem.JoinCode == normalizedJoinCode);

        public Task<bool> TryAddAsync(DomainEvent item, CancellationToken cancellationToken) => Task.FromResult(true);

        public Task UpdateAsync(DomainEvent item, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeParticipantRepository(int participantCount) : IParticipantRepository
    {
        public Task<Participant?> GetByIdAsync(Guid participantId, CancellationToken cancellationToken) =>
            Task.FromResult<Participant?>(null);
        public Task<Participant?> GetBySessionHashAsync(Guid eventId, string sessionCredentialHash, CancellationToken cancellationToken) =>
            Task.FromResult<Participant?>(null);
        public Task<Participant?> GetByEmployeeNumberAsync(Guid eventId, string normalizedEmployeeNumber, CancellationToken cancellationToken) =>
            Task.FromResult<Participant?>(null);
        public Task<IReadOnlyList<Participant>> ListByEventAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Participant>>([]);
        public Task<int> CountByEventAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult(participantCount);
        public Task AddAsync(Participant participant, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateAsync(Participant participant, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakePresenceStore : IParticipantPresenceStore
    {
        public PresenceChange Connect(Guid eventId, Guid participantId, string connectionId) =>
            new(eventId, participantId, true);
        public PresenceChange? Disconnect(string connectionId) => null;
        public IReadOnlySet<Guid> GetOnlineParticipantIds(Guid eventId) => new HashSet<Guid>();
    }

    private sealed class FakeQuizRepository : IQuizRepository
    {
        public Quiz? Quiz { get; set; }
        public QuizQuestion? Question { get; set; }
        public QuizQuestionSession? Session { get; set; }
        public List<QuizLeaderboardRow> LeaderboardRows { get; } = [];
        public QuizQuestionPosition Position { get; set; } = new(1, 1);

        public Task<Quiz?> GetByEventAsync(Guid eventId, CancellationToken cancellationToken) => Task.FromResult(Quiz);
        public Task<Quiz?> GetQuizAsync(Guid quizId, CancellationToken cancellationToken) =>
            Task.FromResult(Quiz?.Id == quizId ? Quiz : null);
        public Task AddQuizAsync(Quiz quiz, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<int> GetNextQuestionOrderAsync(Guid quizId, CancellationToken cancellationToken) => Task.FromResult(2);
        public Task<int> CountQuestionsAsync(Guid quizId, CancellationToken cancellationToken) => Task.FromResult(1);
        public Task<QuizQuestionPosition> GetQuestionPositionAsync(
            Guid quizId,
            Guid questionId,
            QuizQuestionMode mode,
            CancellationToken cancellationToken) => Task.FromResult(Position);
        public Task AddQuestionAsync(QuizQuestion question, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<QuizQuestion?> GetQuestionAsync(Guid questionId, CancellationToken cancellationToken) => Task.FromResult(Question);
        public Task<QuizQuestionSession?> GetCurrentSessionAsync(Guid eventId, CancellationToken cancellationToken) => Task.FromResult(Session);
        public Task AddSessionAsync(QuizQuestionSession session, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task UpdateSessionAsync(QuizQuestionSession session, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task<ParticipantAnswer?> GetAnswerAsync(Guid questionSessionId, Guid participantId, CancellationToken cancellationToken) =>
            Task.FromResult<ParticipantAnswer?>(null);
        public Task<bool> TryAddAnswerAsync(ParticipantAnswer answer, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<int> CountAnswersAsync(Guid questionSessionId, CancellationToken cancellationToken) => Task.FromResult(3);
        public Task<bool> ExecuteRevealAndScoreAsync(Guid eventId, Guid questionSessionId, Func<QuizRevealSnapshot, QuizRevealChanges> calculateChanges, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
        public Task<ParticipantQuestionResult?> GetQuestionResultAsync(Guid questionSessionId, Guid participantId, CancellationToken cancellationToken) =>
            Task.FromResult<ParticipantQuestionResult?>(null);
        public Task<IReadOnlyList<QuizLeaderboardRow>> ListLeaderboardRowsAsync(Guid eventId, Guid quizId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<QuizLeaderboardRow>>(LeaderboardRows);
        public Task<QuizSessionStatisticsData?> GetSessionStatisticsAsync(Guid eventId, Guid questionSessionId, CancellationToken cancellationToken)
        {
            var optionCounts = Question!.Options
                .OrderBy(option => option.Order)
                .Select((option, index) => new QuizOptionAnswerCount(option.Id, index == 1 ? 2 : index == 0 ? 1 : 0))
                .ToArray();
            return Task.FromResult<QuizSessionStatisticsData?>(new QuizSessionStatisticsData(
                Quiz!.Id,
                Question.Id,
                3,
                3,
                2,
                optionCounts));
        }
    }
}
