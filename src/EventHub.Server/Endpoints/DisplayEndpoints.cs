using EventHub.Application.Display;
using EventHub.Domain.Events;
using EventHub.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EventHub.Server.Endpoints;

public static class DisplayEndpoints
{
    public static IEndpointRouteBuilder MapDisplayEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/events/{eventId:guid}/display");

        group.MapGet("/", async (
            Guid eventId,
            int? top,
            IConfiguration configuration,
            DisplayService displayService,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await displayService.GetCurrentStateAsync(
                eventId,
                ResolveLeaderboardTop(top, configuration),
                cancellationToken));
        });

        group.MapPut("/mode", async (
            Guid eventId,
            SetDisplayModeRequest request,
            HttpRequest httpRequest,
            IConfiguration configuration,
            DisplayService displayService,
            IHubContext<PresenceHub, IEventClient> hubContext,
            ILogger<DisplayEndpointLog> logger,
            CancellationToken cancellationToken) =>
        {
            var state = await displayService.SetModeAsync(
                new SetDisplayModeCommand(
                    eventId,
                    request.Mode,
                    httpRequest.Headers["X-Host-Token"].ToString()),
                ResolveLeaderboardTop(null, configuration),
                cancellationToken);
            var notification = new DisplayModeChangedNotification(eventId, state.Mode);
            await Task.WhenAll(
                hubContext.Clients.Group(PresenceHub.DisplayGroup(eventId)).DisplayModeChanged(notification),
                hubContext.Clients.Group(PresenceHub.HostGroup(eventId)).DisplayModeChanged(notification));
            logger.LogInformation(
                "Display mode changed to {DisplayMode} for event {EventId}.",
                state.Mode,
                eventId);
            return Results.Ok(state);
        });

        return endpoints;
    }

    private static int ResolveLeaderboardTop(int? requested, IConfiguration configuration)
    {
        var configured = configuration.GetValue<int?>("EventHub:Display:LeaderboardTop") ?? 10;
        return Math.Clamp(requested ?? configured, 1, 100);
    }

    public sealed record SetDisplayModeRequest(DisplayMode Mode);

    private sealed class DisplayEndpointLog;
}
