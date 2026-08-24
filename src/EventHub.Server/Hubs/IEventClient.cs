using EventHub.Application.Participants;
using EventHub.Application.Quizzes;
using EventHub.Domain.Events;

namespace EventHub.Server.Hubs;

public interface IEventClient
{
    Task ParticipantJoined(ParticipantSummary participant);

    Task ParticipantPresenceChanged(ParticipantSummary participant);

    Task QuestionStarted(QuestionStartedNotification notification);

    Task QuestionProgressUpdated(QuestionProgressNotification notification);

    Task QuestionClosed(QuestionClosedNotification notification);

    Task AnswerRevealed(AnswerRevealedNotification notification);

    Task LeaderboardUpdated(LeaderboardUpdatedNotification notification);

    Task ParticipantCountUpdated(ParticipantCountNotification notification);

    Task DisplayModeChanged(DisplayModeChangedNotification notification);

    Task EventLifecycleChanged(EventLifecycleChangedNotification notification);

    Task EventJoinPolicyChanged(EventJoinPolicyChangedNotification notification);
}

public sealed record QuestionStartedNotification(
    Guid SessionId,
    Guid QuestionId,
    string QuestionText,
    IReadOnlyList<QuizOptionSummary> Options,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset AnswerDeadlineUtc);

public sealed record QuestionProgressNotification(
    Guid SessionId,
    int AnsweredCount,
    int ParticipantCount,
    int OnlineCount);

public sealed record QuestionClosedNotification(Guid SessionId);

public sealed record AnswerRevealedNotification(Guid SessionId, Guid CorrectOptionId);

public sealed record LeaderboardUpdatedNotification(Guid SessionId);

public sealed record ParticipantCountNotification(Guid EventId, int ParticipantCount);

public sealed record DisplayModeChangedNotification(Guid EventId, DisplayMode Mode);

public sealed record EventLifecycleChangedNotification(Guid EventId, EventState State);

public sealed record EventJoinPolicyChangedNotification(Guid EventId, bool IsJoinOpen);
