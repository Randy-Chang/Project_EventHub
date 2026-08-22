using EventHub.Domain.Events;

namespace EventHub.Application.Events;

public sealed record CreateEventCommand(string Name, DateTimeOffset EventDateUtc);

public sealed record EventSummary(
    Guid Id,
    string Name,
    DateTimeOffset EventDateUtc,
    EventState State,
    bool IsJoinOpen);

public sealed record CreateEventResult(EventSummary Event, string HostToken);
