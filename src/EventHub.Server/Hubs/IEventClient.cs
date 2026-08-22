using EventHub.Application.Participants;
using EventHub.Application.Quizzes;

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
