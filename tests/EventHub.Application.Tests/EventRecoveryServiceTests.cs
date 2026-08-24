using EventHub.Application.Abstractions;
using EventHub.Application.Quizzes;
using EventHub.Domain.Quizzes;

namespace EventHub.Application.Tests;

public sealed class EventRecoveryServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 25, 2, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task RecoverExpiredQuestions_ClosesOnlyExpiredOpenSession()
    {
        var expired = CreateOpenSession(Now.AddMinutes(-1), TimeSpan.FromSeconds(10));
        var future = CreateOpenSession(Now, TimeSpan.FromMinutes(1));
        var repository = new RecoveryQuizRepository([expired, future]);
        var service = new EventRecoveryService(repository, new FixedTimeProvider(Now));

        var recovered = await service.RecoverExpiredQuestionsAsync(CancellationToken.None);

        Assert.Equal([expired.Id], recovered);
        Assert.Equal(QuizQuestionState.Closed, expired.State);
        Assert.Equal(QuizQuestionState.Open, future.State);
        Assert.Equal([expired.Id], repository.UpdatedSessionIds);
    }

    private static QuizQuestionSession CreateOpenSession(DateTimeOffset startedAt, TimeSpan duration)
    {
        var session = QuizQuestionSession.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        session.Open(startedAt, duration);
        return session;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class RecoveryQuizRepository(IReadOnlyList<QuizQuestionSession> sessions) : IQuizRepository
    {
        public List<Guid> UpdatedSessionIds { get; } = [];
        public Task<IReadOnlyList<QuizQuestionSession>> ListOpenSessionsAsync(CancellationToken cancellationToken) =>
            Task.FromResult(sessions);
        public Task UpdateSessionAsync(QuizQuestionSession session, CancellationToken cancellationToken)
        {
            UpdatedSessionIds.Add(session.Id);
            return Task.CompletedTask;
        }

        public Task<Quiz?> GetByEventAsync(Guid eventId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<Quiz?> GetQuizAsync(Guid quizId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddQuizAsync(Quiz quiz, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<int> GetNextQuestionOrderAsync(Guid quizId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<int> CountQuestionsAsync(Guid quizId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<QuizQuestionPosition> GetQuestionPositionAsync(Guid quizId, Guid questionId, QuizQuestionMode mode, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddQuestionAsync(QuizQuestion question, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<QuizQuestion?> GetQuestionAsync(Guid questionId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<QuizQuestionSession?> GetCurrentSessionAsync(Guid eventId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task AddSessionAsync(QuizQuestionSession session, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ParticipantAnswer?> GetAnswerAsync(Guid questionSessionId, Guid participantId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> TryAddAnswerAsync(ParticipantAnswer answer, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<int> CountAnswersAsync(Guid questionSessionId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<bool> ExecuteRevealAndScoreAsync(Guid eventId, Guid questionSessionId, Func<QuizRevealSnapshot, QuizRevealChanges> calculateChanges, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ParticipantQuestionResult?> GetQuestionResultAsync(Guid questionSessionId, Guid participantId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<IReadOnlyList<QuizLeaderboardRow>> ListLeaderboardRowsAsync(Guid eventId, Guid quizId, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<QuizSessionStatisticsData?> GetSessionStatisticsAsync(Guid eventId, Guid questionSessionId, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
