using EventHub.Application.Abstractions;
using EventHub.Application.Events;

namespace EventHub.Application.Participants;

public sealed record ParticipantConnectionResult(
    PresenceChange Change,
    ParticipantSummary Participant);

public sealed class ParticipantPresenceService(
    IParticipantPresenceStore presenceStore,
    ParticipantService participantService,
    EventService eventService)
{
    public async Task<ParticipantConnectionResult> ConnectParticipantAsync(
        Guid eventId,
        Guid participantId,
        string sessionToken,
        string connectionId,
        CancellationToken cancellationToken)
    {
        var participant = await participantService.ValidateSessionAsync(
            eventId,
            participantId,
            sessionToken,
            cancellationToken);

        var change = presenceStore.Connect(eventId, participantId, connectionId);
        return new ParticipantConnectionResult(change, participant with { IsOnline = true });
    }

    public PresenceChange? Disconnect(string connectionId)
    {
        return presenceStore.Disconnect(connectionId);
    }

    public Task<bool> IsHostAuthorizedAsync(
        Guid eventId,
        string hostToken,
        CancellationToken cancellationToken)
    {
        return eventService.IsHostAuthorizedAsync(eventId, hostToken, cancellationToken);
    }
}
