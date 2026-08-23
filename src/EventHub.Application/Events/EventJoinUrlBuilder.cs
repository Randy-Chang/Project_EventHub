using EventHub.Domain.Events;

namespace EventHub.Application.Events;

public sealed class EventJoinUrlBuilder
{
    private Uri joinBaseUri;

    public EventJoinUrlBuilder(string joinBaseUrl)
    {
        joinBaseUri = Normalize(joinBaseUrl);
    }

    public bool IsLoopback => Volatile.Read(ref joinBaseUri).IsLoopback;

    public string JoinBaseUrl => Volatile.Read(ref joinBaseUri).AbsoluteUri.TrimEnd('/');

    public void SetJoinBaseUrl(string joinBaseUrl) =>
        Volatile.Write(ref joinBaseUri, Normalize(joinBaseUrl));

    public string Build(string joinCode)
    {
        var normalizedCode = EventJoinCode.Normalize(joinCode);
        return new Uri(
            Volatile.Read(ref joinBaseUri),
            $"join/{Uri.EscapeDataString(normalizedCode)}").AbsoluteUri;
    }

    private static Uri Normalize(string joinBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(joinBaseUrl) ||
            !Uri.TryCreate(joinBaseUrl.Trim(), UriKind.Absolute, out var parsedUri) ||
            (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "EventHub:JoinBaseUrl 必須是有效的 HTTP 或 HTTPS Absolute URI。");
        }

        return new Uri(parsedUri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
    }
}
