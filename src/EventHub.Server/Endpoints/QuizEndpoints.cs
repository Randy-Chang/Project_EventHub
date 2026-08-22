using EventHub.Application.Quizzes;
using EventHub.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EventHub.Server.Endpoints;

public static class QuizEndpoints
{
    public static IEndpointRouteBuilder MapQuizEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/events/{eventId:guid}/quiz");

        group.MapPost("/questions", async (
            Guid eventId,
            CreateQuizQuestionRequest request,
            HttpRequest httpRequest,
            QuizService quizService,
            CancellationToken cancellationToken) =>
        {
            var question = await quizService.CreateQuestionAsync(
                new CreateQuizQuestionCommand(
                    eventId,
                    ReadHostToken(httpRequest),
                    request.QuestionText,
                    request.Options,
                    request.CorrectOptionIndex,
                    request.AnswerDurationSeconds),
                cancellationToken);
            return Results.Created(
                $"/api/v1/events/{eventId}/quiz/questions/{question.Id}",
                question);
        });

        group.MapPost("/questions/{questionId:guid}/start", async (
            Guid eventId,
            Guid questionId,
            HttpRequest httpRequest,
            QuizService quizService,
            IHubContext<PresenceHub, IEventClient> hubContext,
            ILogger<QuizEndpointLog> logger,
            CancellationToken cancellationToken) =>
        {
            var state = await quizService.StartQuestionAsync(
                new StartQuizQuestionCommand(eventId, questionId, ReadHostToken(httpRequest)),
                cancellationToken);
            var notification = new QuestionStartedNotification(
                state.SessionId!.Value,
                state.QuestionId!.Value,
                state.QuestionText!,
                state.Options,
                state.StartedAtUtc!.Value,
                state.AnswerDeadlineUtc!.Value);
            await BroadcastAsync(hubContext, eventId, client => client.QuestionStarted(notification));
            logger.LogInformation(
                "Question {QuestionId} started for event {EventId} with session {SessionId}.",
                questionId,
                eventId,
                state.SessionId);
            return Results.Ok(state);
        });

        group.MapGet("/current", async (
            Guid eventId,
            Guid? participantId,
            HttpRequest request,
            QuizService quizService,
            CancellationToken cancellationToken) =>
        {
            var state = await quizService.GetCurrentStateAsync(
                new QuizStateQuery(
                    eventId,
                    request.Headers["X-Host-Token"].ToString(),
                    participantId,
                    request.Headers["X-Participant-Token"].ToString()),
                cancellationToken);
            return Results.Ok(state);
        });

        group.MapPost("/sessions/{sessionId:guid}/answers", async (
            Guid eventId,
            Guid sessionId,
            SubmitQuizAnswerRequest request,
            HttpRequest httpRequest,
            QuizService quizService,
            IHubContext<PresenceHub, IEventClient> hubContext,
            ILogger<QuizEndpointLog> logger,
            CancellationToken cancellationToken) =>
        {
            var result = await quizService.SubmitAnswerAsync(
                new SubmitQuizAnswerCommand(
                    eventId,
                    sessionId,
                    request.ParticipantId,
                    httpRequest.Headers["X-Participant-Token"].ToString(),
                    request.SelectedOptionId),
                cancellationToken);

            if (result.Status == SubmitQuizAnswerStatus.DeadlinePassed)
            {
                await BroadcastAsync(
                    hubContext,
                    eventId,
                    client => client.QuestionClosed(new QuestionClosedNotification(sessionId)));
                logger.LogWarning(
                    "Answer rejected because session {SessionId} deadline passed.",
                    sessionId);
            }
            else
            {
                await hubContext.Clients.Group(PresenceHub.HostGroup(eventId)).QuestionProgressUpdated(
                    new QuestionProgressNotification(
                        sessionId,
                        result.Progress.AnsweredCount,
                        result.Progress.ParticipantCount,
                        result.Progress.OnlineCount));
            }

            return Results.Ok(result);
        });

        group.MapPost("/sessions/{sessionId:guid}/close", async (
            Guid eventId,
            Guid sessionId,
            HttpRequest httpRequest,
            QuizService quizService,
            IHubContext<PresenceHub, IEventClient> hubContext,
            ILogger<QuizEndpointLog> logger,
            CancellationToken cancellationToken) =>
        {
            var state = await quizService.CloseQuestionAsync(
                new QuizHostCommand(eventId, sessionId, ReadHostToken(httpRequest)),
                cancellationToken);
            await BroadcastAsync(
                hubContext,
                eventId,
                client => client.QuestionClosed(new QuestionClosedNotification(sessionId)));
            logger.LogInformation("Question session {SessionId} closed for event {EventId}.", sessionId, eventId);
            return Results.Ok(state);
        });

        group.MapPost("/sessions/{sessionId:guid}/reveal", async (
            Guid eventId,
            Guid sessionId,
            HttpRequest httpRequest,
            QuizService quizService,
            IHubContext<PresenceHub, IEventClient> hubContext,
            ILogger<QuizEndpointLog> logger,
            CancellationToken cancellationToken) =>
        {
            var state = await quizService.RevealAnswerAsync(
                new QuizHostCommand(eventId, sessionId, ReadHostToken(httpRequest)),
                cancellationToken);
            var notification = new AnswerRevealedNotification(sessionId, state.CorrectOptionId);
            await BroadcastAsync(hubContext, eventId, client => client.AnswerRevealed(notification));
            await BroadcastAsync(
                hubContext,
                eventId,
                client => client.LeaderboardUpdated(new LeaderboardUpdatedNotification(sessionId)));
            logger.LogInformation("Answer revealed for session {SessionId} in event {EventId}.", sessionId, eventId);
            return Results.Ok(state);
        });

        group.MapGet("/leaderboard", async (
            Guid eventId,
            int? top,
            Guid? participantId,
            HttpRequest request,
            QuizScoringService scoringService,
            CancellationToken cancellationToken) =>
        {
            var leaderboard = await scoringService.GetLeaderboardAsync(
                eventId,
                top ?? 10,
                request.Headers["X-Host-Token"].ToString(),
                participantId,
                request.Headers["X-Participant-Token"].ToString(),
                cancellationToken);
            return Results.Ok(leaderboard);
        });

        group.MapGet("/me/score", async (
            Guid eventId,
            Guid participantId,
            HttpRequest request,
            QuizScoringService scoringService,
            CancellationToken cancellationToken) =>
        {
            var score = await scoringService.GetMyScoreAsync(
                eventId,
                participantId,
                request.Headers["X-Participant-Token"].ToString(),
                cancellationToken);
            return Results.Ok(score);
        });

        group.MapGet("/sessions/{sessionId:guid}/stats", async (
            Guid eventId,
            Guid sessionId,
            HttpRequest request,
            QuizScoringService scoringService,
            CancellationToken cancellationToken) =>
        {
            var statistics = await scoringService.GetSessionStatisticsAsync(
                new QuizHostCommand(eventId, sessionId, ReadHostToken(request)),
                cancellationToken);
            return Results.Ok(statistics);
        });

        return endpoints;
    }

    private static string ReadHostToken(HttpRequest request) =>
        request.Headers["X-Host-Token"].ToString();

    private static async Task BroadcastAsync(
        IHubContext<PresenceHub, IEventClient> hubContext,
        Guid eventId,
        Func<IEventClient, Task> notification)
    {
        await Task.WhenAll(
            notification(hubContext.Clients.Group(PresenceHub.HostGroup(eventId))),
            notification(hubContext.Clients.Group(PresenceHub.GuestGroup(eventId))));
    }

    public sealed record CreateQuizQuestionRequest(
        string QuestionText,
        IReadOnlyCollection<string> Options,
        int CorrectOptionIndex,
        int AnswerDurationSeconds);

    public sealed record SubmitQuizAnswerRequest(Guid ParticipantId, Guid SelectedOptionId);

    private sealed class QuizEndpointLog;
}
