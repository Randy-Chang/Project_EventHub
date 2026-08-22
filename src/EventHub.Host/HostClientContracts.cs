namespace EventHub.Host;

internal sealed record CreateEventRequest(string Name, DateTime EventDateUtc);

internal sealed record CreateEventResult(
    EventView Event,
    string HostToken,
    EventJoinInfoView JoinInfo);

internal sealed record EventView(Guid Id, string Name);

internal sealed record EventJoinInfoView(
    Guid EventId,
    string EventName,
    string JoinCode,
    string JoinUrl,
    bool IsJoinOpen,
    bool IsLoopback);

internal sealed record ParticipantView(
    Guid Id,
    string DisplayName,
    string? EmployeeNumber,
    string? Department,
    string? TableNumber,
    bool IsOnline,
    int Score);

internal sealed record CreateQuestionRequest(
    string QuestionText,
    IReadOnlyCollection<string> Options,
    int CorrectOptionIndex,
    int AnswerDurationSeconds);

internal sealed record QuestionView(Guid Id, string Text);

internal sealed record QuizStateView(
    QuizState State,
    Guid? SessionId,
    Guid? QuestionId,
    string? QuestionText,
    IReadOnlyList<QuestionOptionView> Options,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? AnswerDeadlineUtc,
    int AnsweredCount,
    int ParticipantCount,
    int OnlineCount,
    Guid? CorrectOptionId);

internal sealed record QuizStatisticsView(
    int ParticipantCount,
    int AnsweredCount,
    int CorrectCount,
    int IncorrectCount,
    int NoAnswerCount,
    decimal CorrectRate,
    IReadOnlyList<QuizOptionStatisticsView> OptionDistribution);

internal sealed record QuizOptionStatisticsView(Guid OptionId, string Text, int AnswerCount);

internal sealed record QuizLeaderboardView(IReadOnlyList<QuizLeaderboardEntryView> Entries);

internal sealed record QuizLeaderboardEntryView(
    int Rank,
    string DisplayName,
    int TotalScore,
    int CorrectCount,
    int AnsweredCount);

internal sealed record DisplayStateView(DisplayMode Mode);

internal sealed record ProblemResponse(string? Detail);

internal sealed record QuestionProgressNotification(
    Guid SessionId,
    int AnsweredCount,
    int ParticipantCount,
    int OnlineCount);

internal sealed record QuestionStartedNotification(Guid SessionId);

internal sealed record QuestionClosedNotification(Guid SessionId);

internal sealed record AnswerRevealedNotification(Guid SessionId);

internal sealed record LeaderboardUpdatedNotification(Guid SessionId);

internal sealed record DisplayModeChangedNotification(Guid EventId, DisplayMode Mode);

internal enum QuizState
{
    Waiting,
    Open,
    Closed,
    Revealed
}

internal enum DisplayMode
{
    Waiting,
    Question,
    Result,
    Leaderboard
}

internal enum HostConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}
