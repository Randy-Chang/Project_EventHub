namespace EventHub.Application.Participants;

public sealed record JoinParticipantCommand(
    Guid EventId,
    string Name,
    string? Nickname,
    string? EmployeeNumber,
    string? Department,
    string? TableNumber,
    string? SessionToken);

public sealed record ParticipantSummary(
    Guid Id,
    Guid EventId,
    string Name,
    string DisplayName,
    string? EmployeeNumber,
    string? Department,
    string? TableNumber,
    bool IsCheckedIn,
    bool IsOnline,
    bool HasWon,
    int Score);

public sealed record JoinParticipantResult(ParticipantSummary Participant, string SessionToken, bool IsNew);
