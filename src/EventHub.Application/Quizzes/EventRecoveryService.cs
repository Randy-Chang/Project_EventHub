using EventHub.Application.Abstractions;

namespace EventHub.Application.Quizzes;

public sealed class EventRecoveryService(IQuizRepository quizRepository, TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<Guid>> RecoverExpiredQuestionsAsync(
        CancellationToken cancellationToken)
    {
        var recovered = new List<Guid>();
        var now = timeProvider.GetUtcNow();
        foreach (var session in await quizRepository.ListOpenSessionsAsync(cancellationToken))
        {
            if (!session.CloseIfDeadlinePassed(now))
            {
                continue;
            }

            await quizRepository.UpdateSessionAsync(session, cancellationToken);
            recovered.Add(session.Id);
        }

        return recovered;
    }
}
