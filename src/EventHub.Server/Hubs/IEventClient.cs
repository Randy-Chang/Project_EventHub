using EventHub.Application.Participants;

namespace EventHub.Server.Hubs;

public interface IEventClient
{
    Task ParticipantJoined(ParticipantSummary participant);

    Task ParticipantPresenceChanged(ParticipantSummary participant);
}
