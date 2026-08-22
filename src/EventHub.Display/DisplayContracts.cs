namespace EventHub.Display;

public enum DisplayMode
{
    Waiting,
    Question,
    Result,
    Leaderboard
}

public enum QuizQuestionState
{
    Waiting,
    Open,
    Closed,
    Revealed
}

public enum QuizQuestionMode
{
    Practice,
    Scored
}

public enum DisplayPresentationState
{
    WaitingForEvent,
    Joining,
    QuestionOpen,
    QuestionClosed,
    AnswerRevealed,
    Leaderboard,
    Finished
}

public sealed record EventJoinInfoDto(
    Guid EventId,
    string EventName,
    string JoinCode,
    string JoinUrl,
    bool IsJoinOpen,
    bool IsLoopback);

public sealed record QuizOptionDto(Guid Id, string Text, int Order);

public sealed record DisplayQuestionDto(
    Guid SessionId,
    QuizQuestionState State,
    QuizQuestionMode Mode,
    int QuestionNumber,
    int TotalQuestionCount,
    string Text,
    IReadOnlyList<QuizOptionDto> Options,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? AnswerDeadlineUtc,
    int AnsweredCount,
    int ParticipantCount,
    Guid? CorrectOptionId,
    string? Explanation);

public sealed record QuizOptionStatisticsDto(Guid OptionId, string Text, int AnswerCount);

public sealed record QuizSessionStatisticsDto(
    Guid SessionId,
    int ParticipantCount,
    int AnsweredCount,
    int CorrectCount,
    int IncorrectCount,
    int NoAnswerCount,
    decimal CorrectRate,
    IReadOnlyList<QuizOptionStatisticsDto> OptionDistribution);

public sealed record DisplayLeaderboardEntryDto(int Rank, string DisplayName, int TotalScore);

public sealed record CurrentDisplayStateDto(
    Guid EventId,
    string EventName,
    DisplayMode Mode,
    Guid? CurrentQuestionSessionId,
    DateTimeOffset ServerTimeUtc,
    int ParticipantCount,
    EventJoinInfoDto? JoinInfo,
    DisplayQuestionDto? Question,
    QuizSessionStatisticsDto? Statistics,
    IReadOnlyList<DisplayLeaderboardEntryDto> Leaderboard);

public sealed record QuestionProgressNotification(
    Guid SessionId,
    int AnsweredCount,
    int ParticipantCount,
    int OnlineCount);

public sealed record ParticipantCountNotification(Guid EventId, int ParticipantCount);

public sealed record QuestionStartedNotification(Guid SessionId);

public sealed record QuestionClosedNotification(Guid SessionId);

public sealed record AnswerRevealedNotification(Guid SessionId);

public sealed record LeaderboardUpdatedNotification(Guid SessionId);

public sealed record DisplayModeChangedNotification(Guid EventId, DisplayMode Mode);
