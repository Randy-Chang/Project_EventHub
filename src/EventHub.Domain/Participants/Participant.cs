using EventHub.Domain.Common;

namespace EventHub.Domain.Participants;

public sealed class Participant
{
    private Participant()
    {
    }

    private Participant(
        Guid id,
        Guid eventId,
        string name,
        string? nickname,
        string? employeeNumber,
        string? normalizedEmployeeNumber,
        string? department,
        string? tableNumber,
        string sessionCredentialHash,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        EventId = eventId;
        Name = name;
        Nickname = nickname;
        EmployeeNumber = employeeNumber;
        NormalizedEmployeeNumber = normalizedEmployeeNumber;
        Department = department;
        TableNumber = tableNumber;
        SessionCredentialHash = sessionCredentialHash;
        CreatedAtUtc = createdAtUtc;
        LastSeenAtUtc = createdAtUtc;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Nickname { get; private set; }

    public string? EmployeeNumber { get; private set; }

    public string? NormalizedEmployeeNumber { get; private set; }

    public string? Department { get; private set; }

    public string? TableNumber { get; private set; }

    public string SessionCredentialHash { get; private set; } = string.Empty;

    public bool IsCheckedIn { get; private set; }

    public int Score { get; private set; }

    public bool HasWon { get; private set; }

    public DateTimeOffset LastSeenAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public long Version { get; private set; }

    public string DisplayName => string.IsNullOrWhiteSpace(Nickname) ? Name : Nickname;

    public static Participant Create(
        Guid eventId,
        string name,
        string? nickname,
        string? employeeNumber,
        string? department,
        string? tableNumber,
        string sessionCredentialHash,
        DateTimeOffset createdAtUtc)
    {
        var normalizedName = NormalizeRequired(name, "姓名", 100);

        return new Participant(
            Guid.NewGuid(),
            eventId,
            normalizedName,
            NormalizeOptional(nickname, 100),
            NormalizeOptional(employeeNumber, 50),
            NormalizeEmployeeNumber(employeeNumber),
            NormalizeOptional(department, 100),
            NormalizeOptional(tableNumber, 50),
            sessionCredentialHash,
            createdAtUtc);
    }

    public void MarkSeen(DateTimeOffset seenAtUtc)
    {
        if (seenAtUtc > LastSeenAtUtc)
        {
            LastSeenAtUtc = seenAtUtc;
            Version++;
        }
    }

    private static string NormalizeRequired(string value, string fieldName, int maximumLength)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new DomainValidationException($"{fieldName}不可為空白。");
        }

        if (normalized.Length > maximumLength)
        {
            throw new DomainValidationException($"{fieldName}不可超過 {maximumLength} 個字元。");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maximumLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > maximumLength)
        {
            throw new DomainValidationException($"欄位不可超過 {maximumLength} 個字元。");
        }

        return normalized;
    }

    private static string? NormalizeEmployeeNumber(string? employeeNumber)
    {
        var normalized = NormalizeOptional(employeeNumber, 50);
        return normalized?.ToUpperInvariant();
    }
}
