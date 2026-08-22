using EventHub.Domain.Quizzes;

namespace EventHub.Application.Abstractions;

public interface IQuestionBankRepository
{
    Task<bool> TitleExistsAsync(Guid eventId, string title, CancellationToken cancellationToken);

    Task<IReadOnlyList<QuestionBankData>> ListAsync(Guid eventId, CancellationToken cancellationToken);

    Task<QuestionBankData?> GetAsync(Guid eventId, Guid quizId, CancellationToken cancellationToken);

    Task<IReadOnlyList<QuestionBankQuestionData>> ListQuestionsAsync(
        Guid eventId,
        Guid quizId,
        CancellationToken cancellationToken);

    Task ImportAsync(Quiz quiz, IReadOnlyCollection<QuizQuestion> questions, CancellationToken cancellationToken);
}

public sealed record QuestionBankData(Guid Id, string Title, int QuestionCount, DateTimeOffset CreatedAtUtc);

public sealed record QuestionBankQuestionData(QuizQuestion Question, bool IsCurrent, bool WasRevealed);
