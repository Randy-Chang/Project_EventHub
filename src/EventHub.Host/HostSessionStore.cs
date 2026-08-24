using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EventHub.Host;

internal sealed record RecentHostSession(
    string ServerUrl,
    Guid EventId,
    string EventName,
    string HostToken,
    DateTimeOffset LastConnectedAtUtc);

internal interface IHostTokenProtector
{
    string Protect(string value);
    string Unprotect(string value);
}

internal sealed class DpapiHostTokenProtector : IHostTokenProtector
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("EventHub.Host.RecentSession.v1");

    public string Protect(string value) => Convert.ToBase64String(ProtectedData.Protect(
        Encoding.UTF8.GetBytes(value),
        Entropy,
        DataProtectionScope.CurrentUser));

    public string Unprotect(string value) => Encoding.UTF8.GetString(ProtectedData.Unprotect(
        Convert.FromBase64String(value),
        Entropy,
        DataProtectionScope.CurrentUser));
}

internal sealed class HostSessionStore
{
    private readonly string filePath;
    private readonly IHostTokenProtector tokenProtector;

    public HostSessionStore()
        : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EventHub",
                "Host",
                "recent-session.json"),
            new DpapiHostTokenProtector())
    {
    }

    internal HostSessionStore(string filePath, IHostTokenProtector tokenProtector)
    {
        this.filePath = filePath;
        this.tokenProtector = tokenProtector;
    }

    public async Task SaveAsync(RecentHostSession session, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(filePath)
            ?? throw new InvalidOperationException("無法解析最近活動儲存路徑。");
        Directory.CreateDirectory(directory);
        var stored = new StoredRecentHostSession(
            session.ServerUrl,
            session.EventId,
            session.EventName,
            tokenProtector.Protect(session.HostToken),
            session.LastConnectedAtUtc);
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, stored, cancellationToken: cancellationToken);
    }

    public async Task<RecentHostSession?> TryLoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var stored = await JsonSerializer.DeserializeAsync<StoredRecentHostSession>(
                stream,
                cancellationToken: cancellationToken);
            if (stored is null || stored.EventId == Guid.Empty || string.IsNullOrWhiteSpace(stored.EncryptedHostToken))
            {
                return null;
            }

            return new RecentHostSession(
                stored.ServerUrl,
                stored.EventId,
                stored.EventName,
                tokenProtector.Unprotect(stored.EncryptedHostToken),
                stored.LastConnectedAtUtc);
        }
        catch (Exception exception) when (
            exception is JsonException or CryptographicException or FormatException or IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    public Task ForgetAsync()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private sealed record StoredRecentHostSession(
        string ServerUrl,
        Guid EventId,
        string EventName,
        string EncryptedHostToken,
        DateTimeOffset LastConnectedAtUtc);
}
