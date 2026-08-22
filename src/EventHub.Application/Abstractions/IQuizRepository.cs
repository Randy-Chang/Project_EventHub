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

    Task<bool> ExecuteRevealAndScoreAsync(
        Guid eventId,
        Guid questionSessionId,
        Func<QuizRevealSnapshot, QuizRevealChanges> calculateChanges,
        CancellationToken cancellationToken);

    Task<ParticipantQuestionResult?> GetQuestionResultAsync(
        Guid questionSessionId,
        Guid participantId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<QuizLeaderboardRow>> ListLeaderboardRowsAsync(
        Guid eventId,
        Guid quizId,
        CancellationToken cancellationToken);

    Task<QuizSessionStatisticsData?> GetSessionStatisticsAsync(
        Guid eventId,
        Guid questionSessionId,
        CancellationToken cancellationToken);
}

public sealed record QuizRevealSnapshot(
    QuizQuestionSession Session,
    QuizQuestion Question,
    IReadOnlyList<ParticipantAnswer> Answers,
    IReadOnlyList<ParticipantQuizScore> ParticipantScores);

public sealed record QuizRevealChanges(
    IReadOnlyList<ParticipantQuestionResult> QuestionResults,
    IReadOnlyList<ParticipantQuizScore> NewParticipantScores);

public sealed record QuizLeaderboardRow(
    Guid ParticipantId,
    string DisplayName,
    string? Department,
    string? TableNumber,
    int TotalScore,
    int CorrectCount,
    int AnsweredCount);

public sealed record QuizOptionAnswerCount(Guid OptionId, int AnswerCount);

public sealed record QuizSessionStatisticsData(
    Guid QuizId,
    Guid QuestionId,
    int ParticipantCount,
    int AnsweredCount,
    int CorrectCount,
    IReadOnlyList<QuizOptionAnswerCount> OptionCounts);
