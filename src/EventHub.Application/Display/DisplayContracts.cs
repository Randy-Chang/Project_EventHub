using EventHub.Application.Events;
using EventHub.Application.Quizzes;
using EventHub.Domain.Events;
using EventHub.Domain.Quizzes;
using System.Text.Json.Serialization;

namespace EventHub.Application.Display;

public sealed record SetDisplayModeCommand(
    Guid EventId,
    DisplayMode Mode,
    string HostToken);

public sealed record DisplayQuestionState(
    Guid SessionId,
    QuizQuestionState State,
    QuizQuestionMode Mode,
    int QuestionNumber,
    int TotalQuestionCount,
    string Text,
    IReadOnlyList<QuizOptionSummary> Options,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? AnswerDeadlineUtc,
    int AnsweredCount,
    int ParticipantCount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Guid? CorrectOptionId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Explanation);

public sealed record DisplayLeaderboardEntry(
    int Rank,
    string DisplayName,
    int TotalScore);

public sealed record CurrentDisplayState(
    Guid EventId,
    string EventName,
    DisplayMode Mode,
    Guid? CurrentQuestionSessionId,
    DateTimeOffset ServerTimeUtc,
    int ParticipantCount,
    EventJoinInfo? JoinInfo,
    DisplayQuestionState? Question,
    QuizSessionStatistics? Statistics,
    IReadOnlyList<DisplayLeaderboardEntry> Leaderboard);
