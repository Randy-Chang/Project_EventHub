using EventHub.Presentation;

namespace EventHub.Host.Tests;

public sealed class QrCodePngGeneratorTests
{
    [Fact]
    public void Generate_JoinUrl_ReturnsPngImage()
    {
        var generator = new QrCodePngGenerator();

        var image = generator.Generate("http://192.168.10.100:5000/join/8K3F2A");

        Assert.True(image.Length > 100);
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, image[..8]);
    }
}
