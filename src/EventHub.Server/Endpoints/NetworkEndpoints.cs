using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using EventHub.Application.Events;

namespace EventHub.Server.Endpoints;

public static class NetworkEndpoints
{
    public static IEndpointRouteBuilder MapNetworkEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/network");

        group.MapPut("/public-base-url", (
            ConfigurePublicBaseUrlRequest request,
            HttpContext httpContext,
            EventJoinUrlBuilder joinUrlBuilder,
            ILogger<NetworkEndpointLog> logger) =>
        {
            if (!IsLoopback(httpContext.Connection.RemoteIpAddress))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    detail: "網路發布設定只允許從活動電腦本機變更。");
            }

            if (!TryValidateLocalBaseUrl(request.PublicBaseUrl, out var normalizedUrl, out var error))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: error);
            }

            joinUrlBuilder.SetJoinBaseUrl(normalizedUrl);
            logger.LogInformation("Public Join base URL changed to {PublicBaseUrl}.", normalizedUrl);
            return Results.Ok(new NetworkStatusResponse(
                normalizedUrl,
                joinUrlBuilder.IsLoopback));
        });

        return endpoints;
    }

    private static bool TryValidateLocalBaseUrl(
        string value,
        out string normalizedUrl,
        out string error)
    {
        normalizedUrl = string.Empty;
        error = string.Empty;
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttp ||
            !IPAddress.TryParse(uri.Host, out var address))
        {
            error = "Public Base URL 必須是使用本機 IPv4 位址的 HTTP 網址。";
            return false;
        }

        if (address.AddressFamily != AddressFamily.InterNetwork || !IsLocalAddress(address))
        {
            error = "Public Base URL 的 IPv4 位址不屬於此活動電腦。";
            return false;
        }

        normalizedUrl = uri.AbsoluteUri.TrimEnd('/');
        return true;
    }

    private static bool IsLocalAddress(IPAddress address) =>
        IPAddress.IsLoopback(address) ||
        NetworkInterface.GetAllNetworkInterfaces()
            .SelectMany(item => item.GetIPProperties().UnicastAddresses)
            .Any(item => item.Address.Equals(address));

    private static bool IsLoopback(IPAddress? address) =>
        address is not null &&
        IPAddress.IsLoopback(address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address);

    public sealed record ConfigurePublicBaseUrlRequest(string PublicBaseUrl);

    public sealed record NetworkStatusResponse(string PublicBaseUrl, bool IsLoopback);

    private sealed class NetworkEndpointLog;
}
