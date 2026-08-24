using System.Text;

namespace EventHub.Host.Tests;

public sealed class HostSessionStoreTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"eventhub-host-{Guid.NewGuid():N}");

    [Fact]
    public async Task SaveAndLoad_RoundTripsWithoutWritingPlainToken()
    {
        var path = Path.Combine(directory, "recent-session.json");
        var store = new HostSessionStore(path, new ReversibleProtector());
        var expected = new RecentHostSession(
            "http://localhost:5000",
            Guid.NewGuid(),
            "Annual Party",
            "secret-host-token",
            new DateTimeOffset(2026, 8, 25, 1, 0, 0, TimeSpan.Zero));

        await store.SaveAsync(expected);
        var actual = await store.TryLoadAsync();

        Assert.Equal(expected, actual);
        Assert.DoesNotContain("secret-host-token", await File.ReadAllTextAsync(path), StringComparison.Ordinal);
    }

    [Fact]
    public async Task TryLoad_CorruptJson_ReturnsNull()
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "recent-session.json");
        await File.WriteAllTextAsync(path, "{broken");
        var store = new HostSessionStore(path, new ReversibleProtector());

        Assert.Null(await store.TryLoadAsync());
    }

    [Fact]
    public async Task Forget_RemovesOnlyRecentSessionFile()
    {
        var path = Path.Combine(directory, "recent-session.json");
        var unrelated = Path.Combine(directory, "keep.txt");
        var store = new HostSessionStore(path, new ReversibleProtector());
        await store.SaveAsync(new RecentHostSession(
            "http://localhost:5000",
            Guid.NewGuid(),
            "Event",
            "token",
            DateTimeOffset.UtcNow));
        await File.WriteAllTextAsync(unrelated, "keep");

        await store.ForgetAsync();

        Assert.False(File.Exists(path));
        Assert.True(File.Exists(unrelated));
    }

    public void Dispose()
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }
    }

    private sealed class ReversibleProtector : IHostTokenProtector
    {
        public string Protect(string value) => Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        public string Unprotect(string value) => Encoding.UTF8.GetString(Convert.FromBase64String(value));
    }
}
