using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace EventHub.Host;

internal sealed class EventHubHostClient : IAsyncDisposable
{
    private readonly HttpClient httpClient = new();
    private HubConnection? hubConnection;
    private Uri? serverUri;
    private Guid eventId;
    private string hostToken = string.Empty;

    public event Action<ParticipantView>? ParticipantChanged;
    public event Action? QuizStateRefreshRequested;
    public event Action<QuestionProgressNotification>? QuestionProgressChanged;
    public event Action? LeaderboardRefreshRequested;
    public event Action<DisplayMode>? DisplayModeChanged;
    public event Action<HostConnectionState>? ConnectionStateChanged;
    public event Action? RecoveryRequested;

    public async Task<CreateEventResult> CreateEventAsync(
        string serverBaseUrl,
        string name,
        DateTime eventDateUtc,
        CancellationToken cancellationToken = default)
    {
        var baseUri = ParseServerUri(serverBaseUrl);
        using var response = await httpClient.PostAsJsonAsync(
            new Uri(baseUri, "api/v1/events"),
            new CreateEventRequest(name, eventDateUtc),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CreateEventResult>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Server 未回傳活動資料。");
    }

    public async Task ConnectAsync(
        string serverBaseUrl,
        Guid selectedEventId,
        string selectedHostToken,
        CancellationToken cancellationToken = default)
    {
        serverUri = ParseServerUri(serverBaseUrl);
        eventId = selectedEventId;
        hostToken = string.IsNullOrWhiteSpace(selectedHostToken)
            ? throw new InvalidOperationException("Host Token 不可為空白。")
            : selectedHostToken;
        await DisconnectHubAsync();
        ConnectionStateChanged?.Invoke(HostConnectionState.Connecting);
        var query = $"role=host&eventId={eventId:D}&token={Uri.EscapeDataString(hostToken)}";
        hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(serverUri, $"hubs/event?{query}"))
            .WithAutomaticReconnect()
            .Build();
        RegisterHubHandlers(hubConnection);
        await hubConnection.StartAsync(cancellationToken);
        ConnectionStateChanged?.Invoke(HostConnectionState.Connected);
    }

    public Task<EventJoinInfoView> GetJoinInfoAsync(CancellationToken cancellationToken = default) =>
        GetAsync<EventJoinInfoView>($"api/v1/events/{eventId:D}/join-info", true, cancellationToken);

    public Task<List<ParticipantView>> GetParticipantsAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<ParticipantView>>($"api/v1/events/{eventId:D}/participants", true, cancellationToken);

    public Task<QuizStateView> GetQuizStateAsync(CancellationToken cancellationToken = default) =>
        GetAsync<QuizStateView>($"api/v1/events/{eventId:D}/quiz/current", true, cancellationToken);

    public Task<DisplayStateView> GetDisplayStateAsync(CancellationToken cancellationToken = default) =>
        GetAsync<DisplayStateView>($"api/v1/events/{eventId:D}/display", false, cancellationToken);

    public Task<List<QuestionBankSummaryView>> GetQuestionBanksAsync(CancellationToken cancellationToken = default) =>
        GetAsync<List<QuestionBankSummaryView>>(
            $"api/v1/events/{eventId:D}/quiz/question-banks",
            true,
            cancellationToken);

    public Task<List<QuestionBankQuestionView>> GetQuestionsAsync(
        Guid quizId,
        CancellationToken cancellationToken = default) =>
        GetAsync<List<QuestionBankQuestionView>>(
            $"api/v1/events/{eventId:D}/quiz/question-banks/{quizId:D}/questions",
            true,
            cancellationToken);

    public async Task<QuestionView> CreateQuestionAsync(
        CreateQuestionRequest requestBody,
        CancellationToken cancellationToken = default) =>
        await SendJsonAsync<QuestionView>(
            HttpMethod.Post,
            $"api/v1/events/{eventId:D}/quiz/questions",
            requestBody,
            cancellationToken);

    public Task<QuizStateView> StartQuestionAsync(Guid questionId, CancellationToken cancellationToken = default) =>
        SendJsonAsync<QuizStateView>(
            HttpMethod.Post,
            $"api/v1/events/{eventId:D}/quiz/questions/{questionId:D}/start",
            new { },
            cancellationToken);

    public Task<QuizStateView> CloseQuestionAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        SendJsonAsync<QuizStateView>(
            HttpMethod.Post,
            $"api/v1/events/{eventId:D}/quiz/sessions/{sessionId:D}/close",
            new { },
            cancellationToken);

    public async Task RevealAnswerAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        _ = await SendJsonAsync<object>(
            HttpMethod.Post,
            $"api/v1/events/{eventId:D}/quiz/sessions/{sessionId:D}/reveal",
            new { },
            cancellationToken);

    public Task<QuizStatisticsView> GetStatisticsAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default) =>
        GetAsync<QuizStatisticsView>(
            $"api/v1/events/{eventId:D}/quiz/sessions/{sessionId:D}/stats",
            true,
            cancellationToken);

    public Task<QuizLeaderboardView> GetLeaderboardAsync(CancellationToken cancellationToken = default) =>
        GetAsync<QuizLeaderboardView>(
            $"api/v1/events/{eventId:D}/quiz/leaderboard?top=10",
            true,
            cancellationToken);

    public Task<DisplayStateView> SetDisplayModeAsync(
        DisplayMode mode,
        CancellationToken cancellationToken = default) =>
        SendJsonAsync<DisplayStateView>(
            HttpMethod.Put,
            $"api/v1/events/{eventId:D}/display/mode",
            new { mode },
            cancellationToken);

    public Task<QuestionBankPreviewView> PreviewQuestionBankAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        UploadQuestionBankAsync<QuestionBankPreviewView>("quiz-import/preview", filePath, cancellationToken);

    public Task<QuestionBankImportResultView> ImportQuestionBankAsync(
        string filePath,
        CancellationToken cancellationToken = default) =>
        UploadQuestionBankAsync<QuestionBankImportResultView>("quiz-import", filePath, cancellationToken);

    public Task<QuestionBankImportResultView> EnsureDefaultPracticeAsync(
        CancellationToken cancellationToken = default) =>
        SendJsonAsync<QuestionBankImportResultView>(
            HttpMethod.Post,
            $"api/v1/events/{eventId:D}/quiz/default-practice",
            new { },
            cancellationToken);

    public async Task<byte[]> DownloadQuestionBankTemplateAsync(
        string serverBaseUrl,
        CancellationToken cancellationToken = default)
    {
        var baseUri = ParseServerUri(serverBaseUrl);
        using var response = await httpClient.GetAsync(
            new Uri(baseUri, "api/v1/quiz-import/template"),
            cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectHubAsync();
        httpClient.Dispose();
    }

    private void RegisterHubHandlers(HubConnection connection)
    {
        connection.On<ParticipantView>("ParticipantJoined", participant => ParticipantChanged?.Invoke(participant));
        connection.On<ParticipantView>("ParticipantPresenceChanged", participant => ParticipantChanged?.Invoke(participant));
        connection.On<QuestionStartedNotification>("QuestionStarted", _ => QuizStateRefreshRequested?.Invoke());
        connection.On<QuestionProgressNotification>("QuestionProgressUpdated", progress =>
            QuestionProgressChanged?.Invoke(progress));
        connection.On<QuestionClosedNotification>("QuestionClosed", _ => QuizStateRefreshRequested?.Invoke());
        connection.On<AnswerRevealedNotification>("AnswerRevealed", _ => QuizStateRefreshRequested?.Invoke());
        connection.On<LeaderboardUpdatedNotification>("LeaderboardUpdated", _ => LeaderboardRefreshRequested?.Invoke());
        connection.On<DisplayModeChangedNotification>("DisplayModeChanged", notification =>
            DisplayModeChanged?.Invoke(notification.Mode));
        connection.Reconnecting += _ =>
        {
            ConnectionStateChanged?.Invoke(HostConnectionState.Reconnecting);
            return Task.CompletedTask;
        };
        connection.Reconnected += _ =>
        {
            ConnectionStateChanged?.Invoke(HostConnectionState.Connected);
            RecoveryRequested?.Invoke();
            return Task.CompletedTask;
        };
        connection.Closed += _ =>
        {
            ConnectionStateChanged?.Invoke(HostConnectionState.Disconnected);
            return Task.CompletedTask;
        };
    }

    private async Task<T> GetAsync<T>(string path, bool useHostToken, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get, path, useHostToken);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Server 未回傳預期資料。");
    }

    private async Task<T> SendJsonAsync<T>(
        HttpMethod method,
        string path,
        object body,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, path, true);
        request.Content = JsonContent.Create(body);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Server 未回傳預期資料。");
    }

    private async Task<T> UploadQuestionBankAsync<T>(
        string path,
        string filePath,
        CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(filePath);
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", Path.GetFileName(filePath));
        using var request = CreateRequest(HttpMethod.Post, $"api/v1/events/{eventId:D}/{path}", true);
        request.Content = content;
        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Server 未回傳題庫資料。");
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, bool useHostToken)
    {
        var request = new HttpRequestMessage(method, new Uri(GetServerUri(), path));
        if (useHostToken)
        {
            request.Headers.Add("X-Host-Token", hostToken);
        }

        return request;
    }

    private Uri GetServerUri() => serverUri ?? throw new InvalidOperationException("尚未設定 Server URL。");

    private static Uri ParseServerUri(string value)
    {
        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Server URL 必須是有效的 HTTP 或 HTTPS 網址。");
        }

        return new Uri(uri.AbsoluteUri.TrimEnd('/') + "/");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(cancellationToken: cancellationToken);
        throw new InvalidOperationException(problem?.Detail ?? $"Server 回傳 {(int)response.StatusCode}。");
    }

    private async Task DisconnectHubAsync()
    {
        if (hubConnection is null)
        {
            return;
        }

        await hubConnection.DisposeAsync();
        hubConnection = null;
    }
}
