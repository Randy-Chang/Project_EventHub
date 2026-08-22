using EventHub.Application.Participants;
using EventHub.Domain.Common;
using Microsoft.AspNetCore.SignalR;

namespace EventHub.Server.Hubs;

public sealed class PresenceHub(
    ParticipantPresenceService presenceService,
    ILogger<PresenceHub> logger) : Hub<IEventClient>
{
    private const string ParticipantContextKey = "Participant";

    public static string HostGroup(Guid eventId) => $"event:{eventId}:hosts";

    public static string GuestGroup(Guid eventId) => $"event:{eventId}:guests";

    public override async Task OnConnectedAsync()
    {
        var request = Context.GetHttpContext()?.Request
            ?? throw new HubException("無法讀取連線資訊。");
        var eventId = ReadGuid(request, "eventId");
        var role = request.Query["role"].ToString();

        try
        {
            if (string.Equals(role, "host", StringComparison.OrdinalIgnoreCase))
            {
                var hostToken = request.Query["token"].ToString();
                if (!await presenceService.IsHostAuthorizedAsync(
                    eventId,
                    hostToken,
                    Context.ConnectionAborted))
                {
                    throw new UnauthorizedAccessException("Host credential 無效。");
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, HostGroup(eventId));
            }
            else if (string.Equals(role, "guest", StringComparison.OrdinalIgnoreCase))
            {
                var participantId = ReadGuid(request, "participantId");
                var participantToken = request.Query["token"].ToString();
                var result = await presenceService.ConnectParticipantAsync(
                    eventId,
                    participantId,
                    participantToken,
                    Context.ConnectionId,
                    Context.ConnectionAborted);

                Context.Items[ParticipantContextKey] = result.Participant;
                await Groups.AddToGroupAsync(Context.ConnectionId, GuestGroup(eventId));
                await Clients.Group(HostGroup(eventId)).ParticipantPresenceChanged(
                    result.Participant);
            }
            else
            {
                throw new UnauthorizedAccessException("不支援的連線角色。");
            }
        }
        catch (Exception exception) when (
            exception is UnauthorizedAccessException or DomainValidationException or KeyNotFoundException)
        {
            logger.LogWarning("SignalR connection rejected: {Reason}", exception.Message);
            throw new HubException(exception.Message);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var change = presenceService.Disconnect(Context.ConnectionId);
        if (change is not null &&
            Context.Items.TryGetValue(ParticipantContextKey, out var value) &&
            value is ParticipantSummary participant)
        {
            await Clients.Group(HostGroup(change.EventId)).ParticipantPresenceChanged(
                participant with { IsOnline = change.IsOnline });
        }

        if (exception is not null)
        {
            logger.LogInformation(exception, "SignalR connection {ConnectionId} disconnected.", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static Guid ReadGuid(HttpRequest request, string key)
    {
        return Guid.TryParse(request.Query[key], out var value)
            ? value
            : throw new HubException($"{key} 格式無效。");
    }
}
