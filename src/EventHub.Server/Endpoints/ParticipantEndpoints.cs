using EventHub.Application.Participants;
using EventHub.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EventHub.Server.Endpoints;

public static class ParticipantEndpoints
{
    public static IEndpointRouteBuilder MapParticipantEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/events/{eventId:guid}/participants");

        group.MapPost("/join", async (
            Guid eventId,
            JoinParticipantRequest request,
            ParticipantService participantService,
            IHubContext<PresenceHub, IEventClient> hubContext,
            CancellationToken cancellationToken) =>
        {
            var result = await participantService.JoinAsync(
                new JoinParticipantCommand(
                    eventId,
                    request.Name,
                    request.Nickname,
                    request.EmployeeNumber,
                    request.Department,
                    request.TableNumber,
                    request.SessionToken),
                cancellationToken);

            if (result.IsNew)
            {
                var participantCount = await participantService.GetParticipantCountAsync(
                    eventId,
                    cancellationToken);
                await Task.WhenAll(
                    hubContext.Clients
                        .Group(PresenceHub.HostGroup(eventId))
                        .ParticipantJoined(result.Participant),
                    hubContext.Clients
                        .Group(PresenceHub.DisplayGroup(eventId))
                        .ParticipantCountUpdated(new ParticipantCountNotification(eventId, participantCount)));
            }

            return Results.Ok(result);
        });

        group.MapGet("/", async (
            Guid eventId,
            HttpRequest request,
            ParticipantService participantService,
            CancellationToken cancellationToken) =>
        {
            var hostToken = request.Headers["X-Host-Token"].ToString();
            var result = await participantService.ListForHostAsync(eventId, hostToken, cancellationToken);
            return Results.Ok(result);
        });

        group.MapGet("/me", async (
            Guid eventId,
            Guid participantId,
            HttpRequest request,
            ParticipantService participantService,
            CancellationToken cancellationToken) =>
        {
            var participantToken = request.Headers["X-Participant-Token"].ToString();
            var result = await participantService.ValidateSessionAsync(
                eventId,
                participantId,
                participantToken,
                cancellationToken);
            return Results.Ok(result);
        });

        return endpoints;
    }

    public sealed record JoinParticipantRequest(
        string Name,
        string? Nickname,
        string? EmployeeNumber,
        string? Department,
        string? TableNumber,
        string? SessionToken);
}
