using EventHub.Domain.Quizzes;

namespace EventHub.Application.Abstractions;

public interface IQuizRepository
{
    Task<Quiz?> GetByEventAsync(Guid eventId, CancellationToken cancellationToken);

    Task AddQuizAsync(Quiz quiz, CancellationToken cancellationToken);

    Task<int> GetNextQuestionOrderAsync(Guid quizId, CancellationToken cancellationToken);

    Task AddQuestionAsync(QuizQuestion question, CancellationToken cancellationToken);

    Task<QuizQuestion?> GetQuestionAsync(Guid questionId, CancellationToken cancellationToken);

    Task<QuizQuestionSession?> GetCurrentSessionAsync(Guid eventId, CancellationToken cancellationToken);

    Task AddSessionAsync(QuizQuestionSession session, CancellationToken cancellationToken);

    Task UpdateSessionAsync(QuizQuestionSession session, CancellationToken cancellationToken);

    Task<ParticipantAnswer?> GetAnswerAsync(
        Guid questionSessionId,
        Guid participantId,
        CancellationToken cancellationToken);

    Task<bool> TryAddAnswerAsync(ParticipantAnswer answer, CancellationToken cancellationToken);

    Task<int> CountAnswersAsync(Guid questionSessionId, CancellationToken cancellationToken);
}
