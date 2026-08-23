using System.Net;
using System.Net.NetworkInformation;
using EventHub.Host;

namespace EventHub.Host.Tests;

public sealed class NetworkInterfaceDiscoveryTests
{
    [Fact]
    public void LanAddressOption_PrivateEthernetAddress_IsPreferredAndBuildsServerUrl()
    {
        var option = new LanAddressOption(
            "Ethernet",
            IPAddress.Parse("192.168.10.25"),
            NetworkInterfaceType.Ethernet,
            HasDefaultGateway: true);

        Assert.True(option.IsPreferred);
        Assert.Equal("http://192.168.10.25:5000", option.CreatePublicBaseUrl(5000));
        Assert.Contains("Ethernet", option.ToString());
        Assert.Contains("192.168.10.25", option.ToString());
    }

    [Fact]
    public void LanAddressOption_PublicOrTunnelAddress_IsNotPreferred()
    {
        var option = new LanAddressOption(
            "Tunnel",
            IPAddress.Parse("203.0.113.10"),
            NetworkInterfaceType.Tunnel);

        Assert.False(option.IsPreferred);
    }
}
