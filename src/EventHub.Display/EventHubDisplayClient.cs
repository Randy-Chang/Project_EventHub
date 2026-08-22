using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace EventHub.Display;

public sealed class EventHubDisplayClient : IAsyncDisposable
{
    private readonly HttpClient httpClient = new();
    private readonly SemaphoreSlim refreshLock = new(1, 1);
    private readonly SemaphoreSlim reconnectLock = new(1, 1);
    private CancellationTokenSource lifetimeCancellation = new();
    private HubConnection? connection;
    private Uri? serverUri;
    private Guid eventId;
    private int leaderboardTop;

    public event Action<CurrentDisplayStateDto>? StateReceived;

    public event Action<QuestionProgressNotification>? QuestionProgressReceived;

    public event Action<ParticipantCountNotification>? ParticipantCountReceived;

    public event Action<string, bool>? ConnectionStatusChanged;

    public async Task ConnectAsync(
        string serverApiBaseUrl,
        Guid selectedEventId,
        int top,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(serverApiBaseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var parsedUri) ||
            parsedUri.Scheme is not ("http" or "https"))
        {
            throw new InvalidOperationException("Server API URL 必須是有效的 HTTP 或 HTTPS 網址。");
        }

        await DisconnectAsync();
        lifetimeCancellation.Dispose();
        lifetimeCancellation = new CancellationTokenSource();
        serverUri = parsedUri;
        eventId = selectedEventId;
        leaderboardTop = Math.Clamp(top, 1, 100);
        connection = BuildConnection(parsedUri, selectedEventId);
        try
        {
            await connection.StartAsync(cancellationToken);
            await RecoverStateAsync(cancellationToken);
            Trace.TraceInformation("Display connected for event {0}.", selectedEventId);
            ConnectionStatusChanged?.Invoke("已連線", false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            Trace.TraceWarning("Initial Display connection failed: {0}", exception.Message);
            ConnectionStatusChanged?.Invoke("Server 尚未就緒，正在重新連線…", true);
            _ = ReconnectUntilAvailableAsync(lifetimeCancellation.Token);
        }
    }

    public async Task RecoverStateAsync(CancellationToken cancellationToken = default)
    {
        if (serverUri is null || eventId == Guid.Empty)
        {
            return;
        }

        await refreshLock.WaitAsync(cancellationToken);
        try
        {
            var response = await httpClient.GetAsync(
                new Uri(serverUri, $"api/v1/events/{eventId:D}/display?top={leaderboardTop}"),
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(await ReadErrorAsync(response, cancellationToken));
            }

            var state = await response.Content.ReadFromJsonAsync<CurrentDisplayStateDto>(cancellationToken)
                ?? throw new InvalidOperationException("Server 未回傳 Display State。");
            StateReceived?.Invoke(state);
            Trace.TraceInformation("Display state recovered: {0}.", state.Mode);
        }
        finally
        {
            refreshLock.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        lifetimeCancellation.Cancel();
        if (connection is not null)
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
            connection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        lifetimeCancellation.Dispose();
        refreshLock.Dispose();
        reconnectLock.Dispose();
        httpClient.Dispose();
    }

    private HubConnection BuildConnection(Uri baseUri, Guid selectedEventId)
    {
        var hub = new HubConnectionBuilder()
            .WithUrl(new Uri(baseUri, $"hubs/event?role=display&eventId={selectedEventId:D}"))
            .WithAutomaticReconnect([
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30)])
            .Build();

        hub.On<QuestionProgressNotification>(
            "QuestionProgressUpdated",
            notification => QuestionProgressReceived?.Invoke(notification));
        hub.On<ParticipantCountNotification>(
            "ParticipantCountUpdated",
            notification => ParticipantCountReceived?.Invoke(notification));
        hub.On<QuestionStartedNotification>(
            "QuestionStarted",
            notification => _ = RecoverSafelyAsync(lifetimeCancellation.Token));
        hub.On<QuestionClosedNotification>(
            "QuestionClosed",
            notification => _ = RecoverSafelyAsync(lifetimeCancellation.Token));
        hub.On<AnswerRevealedNotification>(
            "AnswerRevealed",
            notification => _ = RecoverSafelyAsync(lifetimeCancellation.Token));
        hub.On<LeaderboardUpdatedNotification>(
            "LeaderboardUpdated",
            notification => _ = RecoverSafelyAsync(lifetimeCancellation.Token));
        hub.On<DisplayModeChangedNotification>(
            "DisplayModeChanged",
            notification => _ = RecoverSafelyAsync(lifetimeCancellation.Token));

        hub.Reconnecting += _ =>
        {
            Trace.TraceWarning("Display SignalR reconnecting.");
            ConnectionStatusChanged?.Invoke("連線中斷，正在重新連線…", true);
            return Task.CompletedTask;
        };
        hub.Reconnected += async _ =>
        {
            Trace.TraceInformation("Display SignalR reconnected.");
            ConnectionStatusChanged?.Invoke("已重新連線", false);
            await RecoverSafelyAsync(lifetimeCancellation.Token);
        };
        hub.Closed += exception =>
        {
            Trace.TraceError("Display SignalR disconnected: {0}", exception?.Message);
            ConnectionStatusChanged?.Invoke("Connection Lost · Reconnecting…", true);
            _ = ReconnectUntilAvailableAsync(lifetimeCancellation.Token);
            return Task.CompletedTask;
        };
        return hub;
    }

    private async Task RecoverSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await RecoverStateAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Trace.TraceError("Failed to load current display state: {0}", exception);
            ConnectionStatusChanged?.Invoke("狀態同步失敗，正在重試…", true);
        }
    }

    private async Task ReconnectUntilAvailableAsync(CancellationToken cancellationToken)
    {
        if (!await reconnectLock.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            while (!cancellationToken.IsCancellationRequested && connection is not null)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                    if (connection.State == HubConnectionState.Disconnected)
                    {
                        await connection.StartAsync(cancellationToken);
                    }

                    if (connection.State != HubConnectionState.Connected)
                    {
                        continue;
                    }

                    await RecoverStateAsync(cancellationToken);
                    ConnectionStatusChanged?.Invoke("已重新連線", false);
                    return;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    Trace.TraceWarning("Display reconnect attempt failed: {0}", exception.Message);
                }
            }
        }
        finally
        {
            reconnectLock.Release();
        }
    }

    private static async Task<string> ReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(cancellationToken);
        return problem?.Detail ?? $"Server 回傳 HTTP {(int)response.StatusCode}。";
    }

    private sealed record ProblemDetailsDto(string? Detail);
}
