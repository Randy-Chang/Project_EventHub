using EventHub.Domain.Common;

namespace EventHub.Domain.Events;

public static class EventJoinCode
{
    public const int Length = 6;
    public const string AllowedCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Normalize(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        if (normalized.Length != Length || normalized.Any(character => !AllowedCharacters.Contains(character)))
        {
            throw new DomainValidationException(
                $"活動加入碼必須是 {Length} 位且只能包含 {AllowedCharacters}。");
        }

        return normalized;
    }
}
