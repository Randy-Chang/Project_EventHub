namespace EventHub.Application.Abstractions;

public sealed record PresenceChange(Guid EventId, Guid ParticipantId, bool IsOnline);

public interface IParticipantPresenceStore
{
    PresenceChange Connect(Guid eventId, Guid participantId, string connectionId);

    PresenceChange? Disconnect(string connectionId);

    IReadOnlySet<Guid> GetOnlineParticipantIds(Guid eventId);
}
