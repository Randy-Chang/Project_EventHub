using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Application.Quizzes;
using EventHub.Domain.Common;
using EventHub.Domain.Events;
using EventHub.Domain.Quizzes;

namespace EventHub.Application.Display;

public sealed class DisplayService(
    IEventRepository eventRepository,
    IParticipantRepository participantRepository,
    IQuizRepository quizRepository,
    EventService eventService,
    QuizScoringService scoringService,
    TimeProvider timeProvider)
{
    public async Task<CurrentDisplayState> SetModeAsync(
        SetDisplayModeCommand command,
        int leaderboardTop,
        CancellationToken cancellationToken)
    {
        if (!await eventService.IsHostAuthorizedAsync(
            command.EventId,
            command.HostToken,
            cancellationToken))
        {
            throw new UnauthorizedAccessException("Host credential 無效。");
        }

        await SetModeCoreAsync(command.EventId, command.Mode, cancellationToken);
        return await GetCurrentStateAsync(command.EventId, leaderboardTop, cancellationToken);
    }

    public Task SetQuestionModeAsync(Guid eventId, CancellationToken cancellationToken) =>
        SetModeCoreAsync(eventId, DisplayMode.Question, cancellationToken);

    public Task SetResultModeAsync(Guid eventId, CancellationToken cancellationToken) =>
        SetModeCoreAsync(eventId, DisplayMode.Result, cancellationToken);

    public async Task<CurrentDisplayState> GetCurrentStateAsync(
        Guid eventId,
        int leaderboardTop,
        CancellationToken cancellationToken)
    {
        var eventItem = await eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException("找不到指定的活動。");
        var now = timeProvider.GetUtcNow();
        var participantCount = await participantRepository.CountByEventAsync(eventId, cancellationToken);
        if (eventItem.DisplayMode == DisplayMode.Waiting)
        {
            return new CurrentDisplayState(
                eventId,
                eventItem.Name,
                DisplayMode.Waiting,
                null,
                now,
                participantCount,
                await eventService.GetJoinInfoAsync(eventId, cancellationToken),
                null,
                null,
                []);
        }

        var session = await quizRepository.GetCurrentSessionAsync(eventId, cancellationToken);
        if (session is null)
        {
            return await BuildWaitingFallbackAsync(eventItem.Id, eventItem.Name, now, participantCount, cancellationToken);
        }

        if (session.CloseIfDeadlinePassed(now))
        {
            await quizRepository.UpdateSessionAsync(session, cancellationToken);
        }

        var question = await quizRepository.GetQuestionAsync(session.QuestionId, cancellationToken)
            ?? throw new KeyNotFoundException("找不到目前展示的題目。");
        var totalQuestionCount = await quizRepository.CountQuestionsAsync(session.QuizId, cancellationToken);
        var answeredCount = await quizRepository.CountAnswersAsync(session.Id, cancellationToken);
        var effectiveMode = eventItem.DisplayMode;
        if (effectiveMode == DisplayMode.Result && session.State != QuizQuestionState.Revealed)
        {
            effectiveMode = DisplayMode.Question;
        }

        var questionState = new DisplayQuestionState(
            session.Id,
            session.State,
            question.Order,
            totalQuestionCount,
            question.Text,
            question.Options
                .OrderBy(option => option.Order)
                .Select(option => new QuizOptionSummary(option.Id, option.Text, option.Order))
                .ToArray(),
            session.StartedAtUtc,
            session.AnswerDeadlineUtc,
            answeredCount,
            participantCount,
            effectiveMode == DisplayMode.Result && session.State == QuizQuestionState.Revealed
                ? question.CorrectOptionId
                : null);

        if (effectiveMode == DisplayMode.Result)
        {
            var statistics = await scoringService.GetPublicSessionStatisticsAsync(
                eventId,
                session.Id,
                cancellationToken);
            return new CurrentDisplayState(
                eventId,
                eventItem.Name,
                effectiveMode,
                session.Id,
                now,
                participantCount,
                null,
                questionState,
                statistics,
                []);
        }

        if (effectiveMode == DisplayMode.Leaderboard)
        {
            var leaderboard = await scoringService.GetPublicLeaderboardAsync(
                eventId,
                leaderboardTop,
                cancellationToken);
            return new CurrentDisplayState(
                eventId,
                eventItem.Name,
                effectiveMode,
                session.Id,
                now,
                participantCount,
                null,
                null,
                null,
                leaderboard.Entries
                    .Select(entry => new DisplayLeaderboardEntry(
                        entry.Rank,
                        entry.DisplayName,
                        entry.TotalScore))
                    .ToArray());
        }

        return new CurrentDisplayState(
            eventId,
            eventItem.Name,
            DisplayMode.Question,
            session.Id,
            now,
            participantCount,
            null,
            questionState,
            null,
            []);
    }

    private async Task SetModeCoreAsync(
        Guid eventId,
        DisplayMode mode,
        CancellationToken cancellationToken)
    {
        var eventItem = await eventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? throw new KeyNotFoundException("找不到指定的活動。");
        if (mode is DisplayMode.Question or DisplayMode.Result)
        {
            var session = await quizRepository.GetCurrentSessionAsync(eventId, cancellationToken)
                ?? throw new DomainValidationException("目前沒有可展示的題目。");
            if (mode == DisplayMode.Result && session.State != QuizQuestionState.Revealed)
            {
                throw new DomainValidationException("答案公布後才能顯示結果畫面。");
            }
        }

        eventItem.SetDisplayMode(mode);
        await eventRepository.UpdateAsync(eventItem, cancellationToken);
    }

    private async Task<CurrentDisplayState> BuildWaitingFallbackAsync(
        Guid eventId,
        string eventName,
        DateTimeOffset now,
        int participantCount,
        CancellationToken cancellationToken)
    {
        return new CurrentDisplayState(
            eventId,
            eventName,
            DisplayMode.Waiting,
            null,
            now,
            participantCount,
            await eventService.GetJoinInfoAsync(eventId, cancellationToken),
            null,
            null,
            []);
    }
}
