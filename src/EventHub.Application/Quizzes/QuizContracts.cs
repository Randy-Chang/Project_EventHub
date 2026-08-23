using EventHub.Domain.Quizzes;
using System.Text.Json.Serialization;

namespace EventHub.Application.Quizzes;

public sealed record CreateQuizQuestionCommand(
    Guid EventId,
    string HostToken,
    string QuestionText,
    IReadOnlyCollection<string> Options,
    int CorrectOptionIndex,
    int AnswerDurationSeconds);

public sealed record QuizQuestionSummary(
    Guid Id,
    string Text,
    IReadOnlyList<QuizOptionSummary> Options,
    int AnswerDurationSeconds,
    int Order,
    QuizQuestionMode Mode);

public sealed record QuizOptionSummary(Guid Id, string Text, int Order);

public sealed record StartQuizQuestionCommand(Guid EventId, Guid QuestionId, string HostToken);

public sealed record QuizHostCommand(Guid EventId, Guid SessionId, string HostToken);

public sealed record SubmitQuizAnswerCommand(
    Guid EventId,
    Guid SessionId,
    Guid ParticipantId,
    string SessionToken,
    Guid SelectedOptionId);

public enum SubmitQuizAnswerStatus
{
    Accepted,
    AlreadyAnswered,
    DeadlinePassed
}

public sealed record SubmitQuizAnswerResult(
    SubmitQuizAnswerStatus Status,
    DateTimeOffset? SubmittedAtUtc,
    QuizProgress Progress);

public sealed record QuizProgress(int AnsweredCount, int ParticipantCount, int OnlineCount);

public sealed record CurrentQuizState(
    QuizQuestionState State,
    Guid? SessionId,
    Guid? QuestionId,
    QuizQuestionMode? Mode,
    string? QuestionText,
    IReadOnlyList<QuizOptionSummary> Options,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? AnswerDeadlineUtc,
    int AnsweredCount,
    int ParticipantCount,
    int OnlineCount,
    Guid? SelectedOptionId,
    bool HasAnswered,
    Guid? CorrectOptionId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Explanation,
    bool? IsCorrect,
    int? BaseScore,
    int? SpeedBonus,
    int? QuestionScore,
    int? TotalScore,
    int? Rank,
    DateTimeOffset ServerTimeUtc = default);

public sealed record QuizStateQuery(
    Guid EventId,
    string? HostToken,
    Guid? ParticipantId,
    string? SessionToken);

public sealed record RevealQuizQuestionResult(
    Guid SessionId,
    Guid CorrectOptionId,
    bool WasAlreadyRevealed);

public sealed record QuizLeaderboardEntry(
    int Rank,
    Guid ParticipantId,
    string DisplayName,
    string? Department,
    string? TableNumber,
    int TotalScore,
    int CorrectCount,
    int AnsweredCount);

public sealed record QuizLeaderboard(
    Guid EventId,
    Guid? QuizId,
    IReadOnlyList<QuizLeaderboardEntry> Entries);

public sealed record ParticipantQuizScoreSummary(
    Guid ParticipantId,
    int TotalScore,
    int CorrectCount,
    int AnsweredCount,
    int Rank);

public sealed record QuizOptionStatistics(Guid OptionId, string Text, int AnswerCount);

public sealed record QuizSessionStatistics(
    Guid SessionId,
    int ParticipantCount,
    int AnsweredCount,
    int CorrectCount,
    int IncorrectCount,
    int NoAnswerCount,
    decimal CorrectRate,
    IReadOnlyList<QuizOptionStatistics> OptionDistribution);
