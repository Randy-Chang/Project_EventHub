using EventHub.Application.Abstractions;

namespace EventHub.Infrastructure.Presence;

public sealed class InMemoryParticipantPresenceStore : IParticipantPresenceStore
{
    private readonly object syncRoot = new();
    private readonly Dictionary<string, ConnectionOwner> ownersByConnection = [];
    private readonly Dictionary<Guid, HashSet<string>> connectionsByParticipant = [];

    public PresenceChange Connect(Guid eventId, Guid participantId, string connectionId)
    {
        lock (syncRoot)
        {
            ownersByConnection[connectionId] = new ConnectionOwner(eventId, participantId);
            if (!connectionsByParticipant.TryGetValue(participantId, out var connections))
            {
                connections = [];
                connectionsByParticipant[participantId] = connections;
            }

            connections.Add(connectionId);
            return new PresenceChange(eventId, participantId, true);
        }
    }

    public PresenceChange? Disconnect(string connectionId)
    {
        lock (syncRoot)
        {
            if (!ownersByConnection.Remove(connectionId, out var owner))
            {
                return null;
            }

            var connections = connectionsByParticipant[owner.ParticipantId];
            connections.Remove(connectionId);
            var isOnline = connections.Count > 0;
            if (!isOnline)
            {
                connectionsByParticipant.Remove(owner.ParticipantId);
            }

            return new PresenceChange(owner.EventId, owner.ParticipantId, isOnline);
        }
    }

    public IReadOnlySet<Guid> GetOnlineParticipantIds(Guid eventId)
    {
        lock (syncRoot)
        {
            return ownersByConnection.Values
                .Where(owner => owner.EventId == eventId)
                .Select(owner => owner.ParticipantId)
                .ToHashSet();
        }
    }

    private sealed record ConnectionOwner(Guid EventId, Guid ParticipantId);
}
