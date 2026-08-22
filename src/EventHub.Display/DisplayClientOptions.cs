using System.Text.Json;

namespace EventHub.Display;

public sealed record DisplayClientOptions(
    string ServerApiBaseUrl,
    string EventId,
    int DisplayScreenIndex,
    bool StartFullscreen,
    int LeaderboardTop)
{
    public static DisplayClientOptions Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            throw new InvalidOperationException("找不到 EventHub.Display appsettings.json。");
        }

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var section = document.RootElement.GetProperty("Display");
        return new DisplayClientOptions(
            section.GetProperty("ServerApiBaseUrl").GetString()
                ?? throw new InvalidOperationException("Display:ServerApiBaseUrl 不可為空白。"),
            section.GetProperty("EventId").GetString() ?? string.Empty,
            section.GetProperty("DisplayScreenIndex").GetInt32(),
            section.GetProperty("StartFullscreen").GetBoolean(),
            Math.Clamp(section.GetProperty("LeaderboardTop").GetInt32(), 1, 100));
    }
}
