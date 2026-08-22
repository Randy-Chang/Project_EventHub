using EventHub.Domain.Events;

namespace EventHub.Application.Events;

public sealed class EventJoinUrlBuilder
{
    private readonly Uri joinBaseUri;

    public EventJoinUrlBuilder(string joinBaseUrl)
    {
        if (string.IsNullOrWhiteSpace(joinBaseUrl) ||
            !Uri.TryCreate(joinBaseUrl.Trim(), UriKind.Absolute, out var parsedUri) ||
            (parsedUri.Scheme != Uri.UriSchemeHttp && parsedUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                "EventHub:JoinBaseUrl 必須是有效的 HTTP 或 HTTPS Absolute URI。");
        }

        joinBaseUri = new Uri(parsedUri.AbsoluteUri.TrimEnd('/') + "/", UriKind.Absolute);
    }

    public bool IsLoopback => joinBaseUri.IsLoopback;

    public string JoinBaseUrl => joinBaseUri.AbsoluteUri.TrimEnd('/');

    public string Build(string joinCode)
    {
        var normalizedCode = EventJoinCode.Normalize(joinCode);
        return new Uri(joinBaseUri, $"join/{Uri.EscapeDataString(normalizedCode)}").AbsoluteUri;
    }
}
