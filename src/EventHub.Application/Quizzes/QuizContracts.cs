using EventHub.Domain.Quizzes;

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
    int Order);

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
    bool? IsCorrect);

public sealed record QuizStateQuery(
    Guid EventId,
    string? HostToken,
    Guid? ParticipantId,
    string? SessionToken);
