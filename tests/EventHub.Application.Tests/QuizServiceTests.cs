using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Application.Participants;
using EventHub.Application.Quizzes;
using EventHub.Domain.Participants;
using EventHub.Domain.Quizzes;
using DomainEvent = EventHub.Domain.Events.Event;

namespace EventHub.Application.Tests;

public sealed class QuizServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 22, 4, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SubmitAnswer_OpenQuestion_AcceptsAnswerUsingServerClock()
    {
        var fixture = CreateFixture(QuizQuestionState.Open);

        var result = await fixture.SubmitAsync(fixture.Participant1, "participant-1-token", fixture.Question.Options.First().Id);

        Assert.Equal(SubmitQuizAnswerStatus.Accepted, result.Status);
        Assert.Equal(Now, result.SubmittedAtUtc);
        Assert.Single(fixture.QuizRepository.Answers);
        Assert.Equal(Now, fixture.QuizRepository.Answers[0].SubmittedAtUtc);
    }

    [Fact]
    public async Task SubmitAnswer_ClosedQuestion_IsRejected()
    {
        var fixture = CreateFixture(QuizQuestionState.Closed);

        var exception = await Assert.ThrowsAsync<QuizApplicationException>(() =>
            fixture.SubmitAsync(fixture.Participant1, "participant-1-token", fixture.Question.Options.First().Id));

        Assert.Equal(QuizErrorCode.InvalidQuestionState, exception.Code);
        Assert.Empty(fixture.QuizRepository.Answers);
    }

    [Fact]
    public async Task SubmitAnswer_DeadlinePassed_ClosesAndRejectsAnswer()
    {
        var fixture = CreateFixture(QuizQuestionState.Open);
        fixture.Clock.UtcNow = Now.AddSeconds(21);

        var result = await fixture.SubmitAsync(fixture.Participant1, "participant-1-token", fixture.Question.Options.First().Id);

        Assert.Equal(SubmitQuizAnswerStatus.DeadlinePassed, result.Status);
        Assert.Equal(QuizQuestionState.Closed, fixture.Session!.State);
        Assert.Empty(fixture.QuizRepository.Answers);
    }

    [Fact]
    public async Task SubmitAnswer_InvalidOption_IsRejected()
    {
        var fixture = CreateFixture(QuizQuestionState.Open);

        var exception = await Assert.ThrowsAsync<QuizApplicationException>(() =>
            fixture.SubmitAsync(fixture.Participant1, "participant-1-token", Guid.NewGuid()));

        Assert.Equal(QuizErrorCode.InvalidOption, exception.Code);
    }

    [Fact]
    public async Task SubmitAnswer_ParticipantFromAnotherEvent_IsRejected()
    {
        var fixture = CreateFixture(QuizQuestionState.Open);
        var outsider = Participant.Create(
            Guid.NewGuid(),
            "Outsider",
            null,
            null,
            null,
            null,
            fixture.Credentials.HashToken("outsider-token"),
            Now);
        fixture.ParticipantRepository.Items.Add(outsider);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            fixture.SubmitAsync(outsider, "outsider-token", fixture.Question.Options.First().Id));

        Assert.Empty(fixture.QuizRepository.Answers);
    }

    [Fact]
    public async Task SubmitAnswer_SameParticipantTwice_ReturnsAlreadyAnsweredAndKeepsOneAnswer()
    {
        var fixture = CreateFixture(QuizQuestionState.Open);
        var optionId = fixture.Question.Options.First().Id;
        _ = await fixture.SubmitAsync(fixture.Participant1, "participant-1-token", optionId);

        var second = await fixture.SubmitAsync(fixture.Participant1, "participant-1-token", optionId);

        Assert.Equal(SubmitQuizAnswerStatus.AlreadyAnswered, second.Status);
        Assert.Single(fixture.QuizRepository.Answers);
    }

    [Fact]
    public async Task SubmitAnswer_DifferentParticipants_CanAnswerSameSession()
    {
        var fixture = CreateFixture(QuizQuestionState.Open);
        var optionId = fixture.Question.Options.First().Id;

        _ = await fixture.SubmitAsync(fixture.Participant1, "participant-1-token", optionId);
        var second = await fixture.SubmitAsync(fixture.Participant2, "participant-2-token", optionId);

        Assert.Equal(SubmitQuizAnswerStatus.Accepted, second.Status);
        Assert.Equal(2, fixture.QuizRepository.Answers.Count);
        Assert.Equal(2, second.Progress.AnsweredCount);
    }

    [Fact]
    public async Task SubmitAnswer_WaitingQuestion_IsRejected()
    {
        var fixture = CreateFixture(QuizQuestionState.Waiting);

        var exception = await Assert.ThrowsAsync<QuizApplicationException>(() =>
            fixture.SubmitAsync(fixture.Participant1, "participant-1-token", fixture.Question.Options.First().Id));

        Assert.Equal(QuizErrorCode.InvalidQuestionState, exception.Code);
    }

    [Fact]
    public async Task SubmitAnswer_DatabaseDuplicateRace_ReturnsAlreadyAnswered()
    {
        var fixture = CreateFixture(QuizQuestionState.Open);
        fixture.QuizRepository.RejectNextAnswerAsDuplicate = true;

        var result = await fixture.SubmitAsync(
            fixture.Participant1,
            "participant-1-token",
            fixture.Question.Options.First().Id);

        Assert.Equal(SubmitQuizAnswerStatus.AlreadyAnswered, result.Status);
        Assert.Empty(fixture.QuizRepository.Answers);
    }

    [Fact]
    public async Task CurrentState_NoSession_ReturnsWaiting()
    {
        var fixture = CreateFixture(null);

        var state = await fixture.GetGuestStateAsync(fixture.Participant1, "participant-1-token");

        Assert.Equal(QuizQuestionState.Waiting, state.State);
        Assert.Null(state.QuestionId);
    }

    [Theory]
    [InlineData(QuizQuestionState.Open)]
    [InlineData(QuizQuestionState.Closed)]
    public async Task CurrentState_BeforeReveal_DoesNotLeakCorrectOption(QuizQuestionState stateValue)
    {
        var fixture = CreateFixture(stateValue);

        var state = await fixture.GetGuestStateAsync(fixture.Participant1, "participant-1-token");

        Assert.Equal(stateValue, state.State);
        Assert.Null(state.CorrectOptionId);
        Assert.Null(state.IsCorrect);
    }

    [Fact]
    public async Task CurrentState_Revealed_ReturnsCorrectOptionAndPersonalResult()
    {
        var fixture = CreateFixture(QuizQuestionState.Open);
        var correctOptionId = fixture.Question.CorrectOptionId;
        _ = await fixture.SubmitAsync(fixture.Participant1, "participant-1-token", correctOptionId);
        fixture.Session!.Close(Now.AddSeconds(1));
        fixture.Session.Reveal(Now.AddSeconds(2));

        var state = await fixture.GetGuestStateAsync(fixture.Participant1, "participant-1-token");

        Assert.Equal(QuizQuestionState.Revealed, state.State);
        Assert.Equal(correctOptionId, state.CorrectOptionId);
        Assert.Equal(correctOptionId, state.SelectedOptionId);
        Assert.True(state.IsCorrect);
    }

    [Fact]
    public async Task StartQuestion_ResponseDoesNotContainCorrectOption()
    {
        var fixture = CreateFixture(null);

        var state = await fixture.Service.StartQuestionAsync(
            new StartQuizQuestionCommand(fixture.EventId, fixture.Question.Id, "host-token"),
            CancellationToken.None);

        Assert.Equal(QuizQuestionState.Open, state.State);
        Assert.Null(state.CorrectOptionId);
    }

    private static Fixture CreateFixture(QuizQuestionState? sessionState)
    {
        var clock = new MutableTimeProvider(Now);
        var credentials = new FakeCredentialService();
        var eventRepository = new FakeEventRepository();
        var participantRepository = new FakeParticipantRepository();
        var presenceStore = new FakePresenceStore();
        var quizRepository = new FakeQuizRepository();
        var eventItem = DomainEvent.Create(
            "Test Event",
            Now.AddDays(1),
            credentials.HashToken("host-token"),
            Now);
        eventRepository.Item = eventItem;

        var participant1 = CreateParticipant(eventItem.Id, "Amy", "participant-1-token", credentials);
        var participant2 = CreateParticipant(eventItem.Id, "Ben", "participant-2-token", credentials);
        participantRepository.Items.AddRange([participant1, participant2]);
        presenceStore.OnlineIds.UnionWith([participant1.Id, participant2.Id]);

        var quiz = Quiz.Create(eventItem.Id, "Quiz", Now);
        var question = QuizQuestion.Create(
            quiz.Id,
            "1 + 1 = ?",
            ["1", "2", "3", "4"],
            1,
            TimeSpan.FromSeconds(20),
            1);
        quizRepository.Quiz = quiz;
        quizRepository.Questions.Add(question);

        QuizQuestionSession? session = null;
        if (sessionState.HasValue)
        {
            session = QuizQuestionSession.Create(eventItem.Id, quiz.Id, question.Id);
            if (sessionState >= QuizQuestionState.Open)
            {
                session.Open(Now, question.AnswerDuration);
            }

            if (sessionState >= QuizQuestionState.Closed)
            {
                session.Close(Now.AddSeconds(1));
            }

            if (sessionState >= QuizQuestionState.Revealed)
            {
                session.Reveal(Now.AddSeconds(2));
            }

            quizRepository.Session = session;
        }

        var eventService = new EventService(eventRepository, credentials, clock);
        var participantService = new ParticipantService(
            participantRepository,
            presenceStore,
            credentials,
            eventService,
            clock);
        var service = new QuizService(
            quizRepository,
            participantRepository,
            presenceStore,
            eventService,
            participantService,
            clock);

        return new Fixture(
            eventItem.Id,
            participant1,
            participant2,
            question,
            session,
            service,
            clock,
            credentials,
            participantRepository,
            quizRepository);
    }

    private static Participant CreateParticipant(
        Guid eventId,
        string name,
        string token,
        ICredentialService credentials) =>
        Participant.Create(eventId, name, null, null, null, null, credentials.HashToken(token), Now);

    private sealed record Fixture(
        Guid EventId,
        Participant Participant1,
        Participant Participant2,
        QuizQuestion Question,
        QuizQuestionSession? Session,
        QuizService Service,
        MutableTimeProvider Clock,
        FakeCredentialService Credentials,
        FakeParticipantRepository ParticipantRepository,
        FakeQuizRepository QuizRepository)
    {
        public Task<SubmitQuizAnswerResult> SubmitAsync(Participant participant, string token, Guid optionId) =>
            Service.SubmitAnswerAsync(
                new SubmitQuizAnswerCommand(EventId, Session!.Id, participant.Id, token, optionId),
                CancellationToken.None);

        public Task<CurrentQuizState> GetGuestStateAsync(Participant participant, string token) =>
            Service.GetCurrentStateAsync(
                new QuizStateQuery(EventId, null, participant.Id, token),
                CancellationToken.None);
    }

    private sealed class MutableTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }

    private sealed class FakeCredentialService : ICredentialService
    {
        public string GenerateToken() => "generated-token-with-at-least-32-characters";

        public string HashToken(string token) => $"hash:{token}";

        public bool Matches(string token, string expectedHash) => HashToken(token) == expectedHash;
    }

    private sealed class FakeEventRepository : IEventRepository
    {
        public DomainEvent? Item { get; set; }

        public Task<DomainEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult(Item?.Id == eventId ? Item : null);

        public Task AddAsync(DomainEvent eventItem, CancellationToken cancellationToken)
        {
            Item = eventItem;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeParticipantRepository : IParticipantRepository
    {
        public List<Participant> Items { get; } = [];

        public Task<Participant?> GetByIdAsync(Guid participantId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(item => item.Id == participantId));

        public Task<Participant?> GetBySessionHashAsync(
            Guid eventId,
            string sessionCredentialHash,
            CancellationToken cancellationToken) =>
            Task.FromResult(Items.SingleOrDefault(item =>
                item.EventId == eventId && item.SessionCredentialHash == sessionCredentialHash));

        public Task<Participant?> GetByEmployeeNumberAsync(
            Guid eventId,
            string normalizedEmployeeNumber,
            CancellationToken cancellationToken) => Task.FromResult<Participant?>(null);

        public Task<IReadOnlyList<Participant>> ListByEventAsync(
            Guid eventId,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Participant>>(Items.Where(item => item.EventId == eventId).ToArray());

        public Task<int> CountByEventAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Count(item => item.EventId == eventId));

        public Task AddAsync(Participant participant, CancellationToken cancellationToken)
        {
            Items.Add(participant);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Participant participant, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakePresenceStore : IParticipantPresenceStore
    {
        public HashSet<Guid> OnlineIds { get; } = [];

        public PresenceChange Connect(Guid eventId, Guid participantId, string connectionId) =>
            new(eventId, participantId, OnlineIds.Add(participantId));

        public PresenceChange? Disconnect(string connectionId) => null;

        public IReadOnlySet<Guid> GetOnlineParticipantIds(Guid eventId) => OnlineIds;
    }

    private sealed class FakeQuizRepository : IQuizRepository
    {
        public Quiz? Quiz { get; set; }

        public List<QuizQuestion> Questions { get; } = [];

        public QuizQuestionSession? Session { get; set; }

        public List<ParticipantAnswer> Answers { get; } = [];

        public bool RejectNextAnswerAsDuplicate { get; set; }

        public Task<Quiz?> GetByEventAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult(Quiz?.EventId == eventId ? Quiz : null);

        public Task AddQuizAsync(Quiz quiz, CancellationToken cancellationToken)
        {
            Quiz = quiz;
            return Task.CompletedTask;
        }

        public Task<int> GetNextQuestionOrderAsync(Guid quizId, CancellationToken cancellationToken) =>
            Task.FromResult(Questions.Count + 1);

        public Task AddQuestionAsync(QuizQuestion question, CancellationToken cancellationToken)
        {
            Questions.Add(question);
            return Task.CompletedTask;
        }

        public Task<QuizQuestion?> GetQuestionAsync(Guid questionId, CancellationToken cancellationToken) =>
            Task.FromResult(Questions.SingleOrDefault(question => question.Id == questionId));

        public Task<QuizQuestionSession?> GetCurrentSessionAsync(Guid eventId, CancellationToken cancellationToken) =>
            Task.FromResult(Session?.EventId == eventId ? Session : null);

        public Task AddSessionAsync(QuizQuestionSession session, CancellationToken cancellationToken)
        {
            Session = session;
            return Task.CompletedTask;
        }

        public Task UpdateSessionAsync(QuizQuestionSession session, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<ParticipantAnswer?> GetAnswerAsync(
            Guid questionSessionId,
            Guid participantId,
            CancellationToken cancellationToken) =>
            Task.FromResult(Answers.SingleOrDefault(answer =>
                answer.QuestionSessionId == questionSessionId && answer.ParticipantId == participantId));

        public Task<bool> TryAddAnswerAsync(ParticipantAnswer answer, CancellationToken cancellationToken)
        {
            if (RejectNextAnswerAsDuplicate)
            {
                RejectNextAnswerAsDuplicate = false;
                return Task.FromResult(false);
            }

            if (Answers.Any(item =>
                item.QuestionSessionId == answer.QuestionSessionId && item.ParticipantId == answer.ParticipantId))
            {
                return Task.FromResult(false);
            }

            Answers.Add(answer);
            return Task.FromResult(true);
        }

        public Task<int> CountAnswersAsync(Guid questionSessionId, CancellationToken cancellationToken) =>
            Task.FromResult(Answers.Count(answer => answer.QuestionSessionId == questionSessionId));
    }
}
