using EventHub.Application.Display;
using EventHub.Application.Participants;
using EventHub.Application.Quizzes;
using EventHub.Domain.Events;

namespace EventHub.Application.Events;

public enum HostRecommendedAction
{
    PrepareEvent,
    OpenJoining,
    StartEvent,
    StartQuestion,
    CloseQuestion,
    RevealAnswer,
    ContinueQuiz,
    ShowFinalLeaderboard,
    CompleteEvent,
    ViewCompletedEvent
}

public sealed record HostSessionSnapshot(
    EventSummary Event,
    EventJoinInfo JoinInfo,
    IReadOnlyList<ParticipantSummary> Participants,
    IReadOnlyList<QuestionBankSummary> QuestionBanks,
    CurrentQuizState Quiz,
    CurrentDisplayState Display,
    QuizSessionStatistics? Statistics,
    QuizLeaderboard? Leaderboard,
    HostRecommendedAction RecommendedAction,
    bool WasDeadlineRecovered,
    DateTimeOffset ServerTimeUtc);
