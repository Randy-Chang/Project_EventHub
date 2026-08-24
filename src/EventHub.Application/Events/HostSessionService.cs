using EventHub.Application.Abstractions;
using EventHub.Application.Display;
using EventHub.Application.Participants;
using EventHub.Application.Quizzes;
using EventHub.Domain.Events;
using EventHub.Domain.Quizzes;

namespace EventHub.Application.Events;

public sealed class HostSessionService(
    IQuizRepository quizRepository,
    EventService eventService,
    ParticipantService participantService,
    QuestionBankService questionBankService,
    QuizService quizService,
    QuizScoringService scoringService,
    DisplayService displayService,
    TimeProvider timeProvider)
{
    public async Task<HostSessionSnapshot> GetAsync(
        Guid eventId,
        string hostToken,
        int leaderboardTop,
        CancellationToken cancellationToken)
    {
        if (!await eventService.IsHostAuthorizedAsync(eventId, hostToken, cancellationToken))
        {
            throw new UnauthorizedAccessException("Host credential 無效。");
        }

        var beforeRecovery = await quizRepository.GetCurrentSessionAsync(eventId, cancellationToken);
        var now = timeProvider.GetUtcNow();
        var wasDeadlineRecovered = beforeRecovery is
        {
            State: QuizQuestionState.Open,
            AnswerDeadlineUtc: not null
        } && beforeRecovery.AnswerDeadlineUtc <= now;

        var eventSummary = await eventService.GetAsync(eventId, cancellationToken);
        var quizState = await quizService.GetCurrentStateAsync(
            new QuizStateQuery(eventId, hostToken, null, null),
            cancellationToken);
        var participants = await participantService.ListForHostAsync(eventId, hostToken, cancellationToken);
        var questionBanks = await questionBankService.ListAsync(eventId, hostToken, cancellationToken);
        var display = await displayService.GetCurrentStateAsync(eventId, leaderboardTop, cancellationToken);
        QuizSessionStatistics? statistics = null;
        QuizLeaderboard? leaderboard = null;
        if (quizState.State == QuizQuestionState.Revealed && quizState.SessionId.HasValue)
        {
            statistics = await scoringService.GetSessionStatisticsAsync(
                new QuizHostCommand(eventId, quizState.SessionId.Value, hostToken),
                cancellationToken);
            leaderboard = await scoringService.GetLeaderboardAsync(
                eventId,
                leaderboardTop,
                hostToken,
                null,
                null,
                cancellationToken);
        }

        return new HostSessionSnapshot(
            eventSummary,
            await eventService.GetJoinInfoForHostAsync(eventId, hostToken, cancellationToken),
            participants,
            questionBanks,
            quizState,
            display,
            statistics,
            leaderboard,
            ResolveRecommendedAction(eventSummary, quizState, questionBanks),
            wasDeadlineRecovered,
            timeProvider.GetUtcNow());
    }

    private static HostRecommendedAction ResolveRecommendedAction(
        EventSummary eventSummary,
        CurrentQuizState quizState,
        IReadOnlyList<QuestionBankSummary> questionBanks)
    {
        if (eventSummary.State == EventState.Completed)
        {
            return HostRecommendedAction.ViewCompletedEvent;
        }

        if (eventSummary.State == EventState.Draft)
        {
            return HostRecommendedAction.PrepareEvent;
        }

        if ((eventSummary.State is EventState.Ready or EventState.Active) && !eventSummary.IsJoinOpen)
        {
            return HostRecommendedAction.OpenJoining;
        }

        if (eventSummary.State == EventState.Ready)
        {
            return HostRecommendedAction.StartEvent;
        }

        return quizState.State switch
        {
            QuizQuestionState.Open => HostRecommendedAction.CloseQuestion,
            QuizQuestionState.Closed => HostRecommendedAction.RevealAnswer,
            QuizQuestionState.Revealed => HostRecommendedAction.ContinueQuiz,
            _ when questionBanks.Count > 0 => HostRecommendedAction.StartQuestion,
            _ => HostRecommendedAction.CompleteEvent
        };
    }
}
