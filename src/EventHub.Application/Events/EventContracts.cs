using EventHub.Domain.Events;

namespace EventHub.Application.Events;

public sealed record CreateEventCommand(string Name, DateTimeOffset EventDateUtc);

public sealed record ChangeEventStateCommand(Guid EventId, string HostToken, EventState TargetState);

public sealed record ChangeJoinPolicyCommand(Guid EventId, string HostToken, bool IsJoinOpen);

public sealed record EventSummary(
    Guid Id,
    string Name,
    string JoinCode,
    DateTimeOffset EventDateUtc,
    EventState State,
    bool IsJoinOpen);

public sealed record EventJoinInfo(
    Guid EventId,
    string EventName,
    string JoinCode,
    string JoinUrl,
    bool IsJoinOpen,
    bool IsLoopback);

public sealed record CreateEventResult(
    EventSummary Event,
    string HostToken,
    EventJoinInfo JoinInfo);
