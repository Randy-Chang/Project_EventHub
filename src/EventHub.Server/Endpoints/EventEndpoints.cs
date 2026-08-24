using EventHub.Application.Events;
using EventHub.Domain.Events;
using EventHub.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EventHub.Server.Endpoints;

public static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/events");

        group.MapPost("/", async (
            CreateEventRequest request,
            EventService eventService,
            CancellationToken cancellationToken) =>
        {
            var result = await eventService.CreateAsync(
                new CreateEventCommand(request.Name, request.EventDateUtc),
                cancellationToken);
            return Results.Created($"/api/v1/events/{result.Event.Id}", result);
        });

        group.MapGet("/{eventId:guid}", async (
            Guid eventId,
            EventService eventService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await eventService.GetAsync(eventId, cancellationToken));
        });

        group.MapGet("/{eventId:guid}/join-info", async (
            Guid eventId,
            HttpRequest request,
            EventService eventService,
            CancellationToken cancellationToken) =>
        {
            var hostToken = request.Headers["X-Host-Token"].ToString();
            return Results.Ok(await eventService.GetJoinInfoForHostAsync(
                eventId,
                hostToken,
                cancellationToken));
        });

        group.MapGet("/join/{joinCode}", async (
            string joinCode,
            EventService eventService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await eventService.ResolveJoinCodeAsync(joinCode, cancellationToken));
        });

        group.MapPut("/{eventId:guid}/state", async (
            Guid eventId,
            ChangeEventStateRequest body,
            HttpRequest request,
            EventService eventService,
            IHubContext<PresenceHub, IEventClient> hubContext,
            ILogger<EventEndpointLog> logger,
            CancellationToken cancellationToken) =>
        {
            var result = await eventService.ChangeStateAsync(
                new ChangeEventStateCommand(eventId, ReadHostToken(request), body.State),
                cancellationToken);
            await BroadcastAsync(
                hubContext,
                eventId,
                client => client.EventLifecycleChanged(new EventLifecycleChangedNotification(eventId, result.State)));
            logger.LogInformation(
                "Event {EventId} lifecycle changed to {EventState}.",
                eventId,
                result.State);
            return Results.Ok(result);
        });

        group.MapPut("/{eventId:guid}/join-policy", async (
            Guid eventId,
            ChangeJoinPolicyRequest body,
            HttpRequest request,
            EventService eventService,
            IHubContext<PresenceHub, IEventClient> hubContext,
            CancellationToken cancellationToken) =>
        {
            var result = await eventService.ChangeJoinPolicyAsync(
                new ChangeJoinPolicyCommand(eventId, ReadHostToken(request), body.IsJoinOpen),
                cancellationToken);
            await BroadcastAsync(
                hubContext,
                eventId,
                client => client.EventJoinPolicyChanged(new EventJoinPolicyChangedNotification(eventId, result.IsJoinOpen)));
            return Results.Ok(result);
        });

        group.MapGet("/{eventId:guid}/host-session", async (
            Guid eventId,
            HttpRequest request,
            IConfiguration configuration,
            HostSessionService hostSessionService,
            CancellationToken cancellationToken) =>
        {
            var leaderboardTop = Math.Clamp(
                configuration.GetValue<int?>("EventHub:Display:LeaderboardTop") ?? 10,
                1,
                100);
            return Results.Ok(await hostSessionService.GetAsync(
                eventId,
                ReadHostToken(request),
                leaderboardTop,
                cancellationToken));
        });

        return endpoints;
    }

    public sealed record CreateEventRequest(string Name, DateTimeOffset EventDateUtc);

    public sealed record ChangeEventStateRequest(EventState State);

    public sealed record ChangeJoinPolicyRequest(bool IsJoinOpen);

    private static string ReadHostToken(HttpRequest request) => request.Headers["X-Host-Token"].ToString();

    private static async Task BroadcastAsync(
        IHubContext<PresenceHub, IEventClient> hubContext,
        Guid eventId,
        Func<IEventClient, Task> notification)
    {
        await Task.WhenAll(
            notification(hubContext.Clients.Group(PresenceHub.HostGroup(eventId))),
            notification(hubContext.Clients.Group(PresenceHub.GuestGroup(eventId))),
            notification(hubContext.Clients.Group(PresenceHub.DisplayGroup(eventId))));
    }

    private sealed class EventEndpointLog;
}
