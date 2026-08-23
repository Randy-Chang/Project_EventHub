using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace EventHub.Host;

internal sealed record LanAddressOption(
    string InterfaceName,
    IPAddress Address,
    NetworkInterfaceType InterfaceType,
    bool HasDefaultGateway = false)
{
    public bool IsPreferred =>
        InterfaceType is NetworkInterfaceType.Ethernet or NetworkInterfaceType.Wireless80211 &&
        IsPrivate(Address) &&
        HasDefaultGateway;

    public string CreatePublicBaseUrl(int port) => $"http://{Address}:{port}";

    public override string ToString() => $"{InterfaceName} — {Address}";

    private static bool IsPrivate(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        return bytes[0] == 10 ||
            bytes[0] == 172 && bytes[1] is >= 16 and <= 31 ||
            bytes[0] == 192 && bytes[1] == 168;
    }
}

internal sealed class NetworkInterfaceDiscovery
{
    public IReadOnlyList<LanAddressOption> GetAvailableAddresses() =>
        NetworkInterface.GetAllNetworkInterfaces()
            .Where(item =>
                item.OperationalStatus == OperationalStatus.Up &&
                item.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .SelectMany(item => item.GetIPProperties().UnicastAddresses
                .Where(address => IsUsable(address.Address))
                .Select(address => new LanAddressOption(
                    item.Name,
                    address.Address,
                    item.NetworkInterfaceType,
                    item.GetIPProperties().GatewayAddresses.Any(gateway =>
                        gateway.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !gateway.Address.Equals(IPAddress.Any)))))
            .OrderByDescending(item => item.IsPreferred)
            .ThenBy(item => item.InterfaceName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(item => item.Address.ToString(), StringComparer.Ordinal)
            .ToArray();

    private static bool IsUsable(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(address))
        {
            return false;
        }

        var bytes = address.GetAddressBytes();
        return bytes[0] != 169 || bytes[1] != 254;
    }
}
