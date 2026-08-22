using System.Net.Http.Json;
using System.Runtime.InteropServices;
using EventHub.Presentation;
using Microsoft.AspNetCore.SignalR.Client;

namespace EventHub.Host;

public partial class HostDashboardForm : Form
{
    private readonly HttpClient httpClient = new();
    private readonly QrCodePngGenerator qrCodeGenerator = new();
    private HubConnection? hubConnection;
    private Guid? selectedQuestionId;
    private Guid? currentSessionId;
    private CancellationTokenSource? quizDeadlineRefreshCancellation;

    public HostDashboardForm()
    {
        InitializeComponent();
    }

    private async void createEventButton_Click(object? sender, EventArgs e)
    {
        await RunUiOperationAsync(async () =>
        {
            var serverUri = GetServerUri();
            var response = await httpClient.PostAsJsonAsync(
                new Uri(serverUri, "api/v1/events"),
                new CreateEventRequest(eventNameTextBox.Text, eventDatePicker.Value.ToUniversalTime()));
            await EnsureSuccessAsync(response);

            var result = await response.Content.ReadFromJsonAsync<CreateEventResult>()
                ?? throw new InvalidOperationException("Server 未回傳活動資料。");
            eventIdTextBox.Text = result.Event.Id.ToString();
            hostTokenTextBox.Text = result.HostToken;
            ApplyJoinInfo(result.JoinInfo);
            statusLabel.Text = $"已建立：{result.Event.Name}";
            await ConnectToEventAsync();
        });
    }

    private async void connectButton_Click(object? sender, EventArgs e)
    {
        await RunUiOperationAsync(ConnectToEventAsync);
    }

    private async Task ConnectToEventAsync()
    {
        if (!Guid.TryParse(eventIdTextBox.Text, out var eventId))
        {
            throw new InvalidOperationException("活動 ID 格式不正確。");
        }

        if (string.IsNullOrWhiteSpace(hostTokenTextBox.Text))
        {
            throw new InvalidOperationException("Host Token 不可為空白。");
        }

        await DisconnectHubAsync();
        var serverUri = GetServerUri();
        var query = $"role=host&eventId={eventId:D}&token={Uri.EscapeDataString(hostTokenTextBox.Text)}";
        hubConnection = new HubConnectionBuilder()
            .WithUrl(new Uri(serverUri, $"hubs/event?{query}"))
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<ParticipantView>("ParticipantJoined", UpsertParticipantThreadSafe);
        hubConnection.On<ParticipantView>("ParticipantPresenceChanged", UpsertParticipantThreadSafe);
        hubConnection.On<QuestionStartedNotification>("QuestionStarted", _ => RefreshQuizStateThreadSafe(eventId));
        hubConnection.On<QuestionProgressNotification>("QuestionProgressUpdated", UpdateQuizProgressThreadSafe);
        hubConnection.On<QuestionClosedNotification>("QuestionClosed", _ => RefreshQuizStateThreadSafe(eventId));
        hubConnection.On<AnswerRevealedNotification>("AnswerRevealed", _ => RefreshQuizStateThreadSafe(eventId));
        hubConnection.On<LeaderboardUpdatedNotification>("LeaderboardUpdated", _ => RefreshQuizResultsThreadSafe(eventId));
        hubConnection.On<DisplayModeChangedNotification>("DisplayModeChanged", notification =>
            UpdateDisplayModeThreadSafe(notification.Mode));
        hubConnection.Reconnecting += _ => UpdateStatusAsync("SignalR 重新連線中…");
        hubConnection.Reconnected += _ =>
        {
            UpdateStatusThreadSafe("SignalR 已重新連線");
            BeginInvoke(async () =>
            {
                await LoadParticipantsAsync(eventId);
                await LoadQuizStateAsync(eventId);
                await LoadJoinInfoAsync(eventId);
                await LoadDisplayStateAsync(eventId);
            });
            return Task.CompletedTask;
        };
        hubConnection.Closed += _ => UpdateStatusAsync("SignalR 已離線");

        await hubConnection.StartAsync();
        await LoadParticipantsAsync(eventId);
        await LoadQuizStateAsync(eventId);
        await LoadJoinInfoAsync(eventId);
        await LoadDisplayStateAsync(eventId);
        statusLabel.Text = "已連線，正在監看參與者";
    }

    private async Task LoadJoinInfoAsync(Guid eventId)
    {
        using var request = CreateHostRequest(
            HttpMethod.Get,
            $"api/v1/events/{eventId:D}/join-info");
        using var response = await httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
        var joinInfo = await response.Content.ReadFromJsonAsync<EventJoinInfoView>()
            ?? throw new InvalidOperationException("Server 未回傳活動加入資訊。");
        ApplyJoinInfo(joinInfo);
    }

    private void ApplyJoinInfo(EventJoinInfoView joinInfo)
    {
        joinEventNameLabel.Text = $"活動：{joinInfo.EventName}";
        joinCodeValueLabel.Text = joinInfo.JoinCode;
        joinUrlTextBox.Text = joinInfo.JoinUrl;
        joinUrlWarningLabel.Text = joinInfo.IsLoopback
            ? "警告：目前使用 localhost，其他手機無法連線。"
            : "此網址應由連接同一 Wi-Fi 的手機開啟。";
        joinUrlWarningLabel.ForeColor = joinInfo.IsLoopback ? Color.Firebrick : Color.DarkGreen;

        var pngBytes = qrCodeGenerator.Generate(joinInfo.JoinUrl);
        using var stream = new MemoryStream(pngBytes);
        using var sourceImage = Image.FromStream(stream);
        var replacement = new Bitmap(sourceImage);
        var previous = joinQrCodePictureBox.Image;
        joinQrCodePictureBox.Image = replacement;
        previous?.Dispose();
    }

    private void copyJoinUrlButton_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(joinUrlTextBox.Text))
            {
                throw new InvalidOperationException("目前沒有可複製的加入網址。");
            }

            Clipboard.SetText(joinUrlTextBox.Text);
            statusLabel.Text = "加入網址已複製。";
        }
        catch (Exception exception) when (exception is ExternalException or InvalidOperationException)
        {
            statusLabel.Text = $"複製加入網址失敗：{exception.Message}";
        }
    }

    private async void createQuestionButton_Click(object? sender, EventArgs e)
    {
        await RunUiOperationAsync(async () =>
        {
            var eventId = GetEventId();
            var options = new[]
            {
                optionATextBox.Text,
                optionBTextBox.Text,
                optionCTextBox.Text,
                optionDTextBox.Text
            }.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();

            if (correctOptionComboBox.SelectedIndex < 0 || correctOptionComboBox.SelectedIndex >= options.Length)
            {
                throw new InvalidOperationException("請選擇存在的正確答案選項。");
            }

            using var request = CreateHostRequest(
                HttpMethod.Post,
                $"api/v1/events/{eventId:D}/quiz/questions");
            request.Content = JsonContent.Create(new CreateQuestionRequest(
                questionTextBox.Text,
                options,
                correctOptionComboBox.SelectedIndex,
                (int)answerDurationNumeric.Value));
            using var response = await httpClient.SendAsync(request);
            await EnsureSuccessAsync(response);
            var question = await response.Content.ReadFromJsonAsync<QuestionView>()
                ?? throw new InvalidOperationException("Server 未回傳題目資料。");
            selectedQuestionId = question.Id;
            currentQuestionLabel.Text = $"目前題目：{question.Text}";
            quizStateLabel.Text = "狀態：Waiting（題目已建立）";
            UpdateQuizButtons(QuizState.Waiting);
        });
    }

    private async void startQuestionButton_Click(object? sender, EventArgs e)
    {
        await RunUiOperationAsync(async () =>
        {
            if (!selectedQuestionId.HasValue)
            {
                throw new InvalidOperationException("請先建立題目。");
            }

            var eventId = GetEventId();
            using var request = CreateHostRequest(
                HttpMethod.Post,
                $"api/v1/events/{eventId:D}/quiz/questions/{selectedQuestionId.Value:D}/start");
            request.Content = JsonContent.Create(new { });
            using var response = await httpClient.SendAsync(request);
            await EnsureSuccessAsync(response);
            var state = await response.Content.ReadFromJsonAsync<QuizStateView>()
                ?? throw new InvalidOperationException("Server 未回傳 Quiz 狀態。");
            ApplyQuizState(state);
            displayModeValueLabel.Text = "目前畫面：Question";
        });
    }

    private async void closeQuestionButton_Click(object? sender, EventArgs e)
    {
        await ExecuteSessionCommandAsync("close");
    }

    private async void revealAnswerButton_Click(object? sender, EventArgs e)
    {
        await RunUiOperationAsync(async () =>
        {
            if (!currentSessionId.HasValue)
            {
                throw new InvalidOperationException("目前沒有題目場次。");
            }

            var eventId = GetEventId();
            using var request = CreateHostRequest(
                HttpMethod.Post,
                $"api/v1/events/{eventId:D}/quiz/sessions/{currentSessionId.Value:D}/reveal");
            request.Content = JsonContent.Create(new { });
            using var response = await httpClient.SendAsync(request);
            await EnsureSuccessAsync(response);
            await LoadQuizStateAsync(eventId);
            displayModeValueLabel.Text = "目前畫面：Result";
        });
    }

    private async void showWaitingButton_Click(object? sender, EventArgs e)
    {
        await SetDisplayModeAsync(DisplayMode.Waiting);
    }

    private async void showQuestionButton_Click(object? sender, EventArgs e)
    {
        await SetDisplayModeAsync(DisplayMode.Question);
    }

    private async void showResultButton_Click(object? sender, EventArgs e)
    {
        await SetDisplayModeAsync(DisplayMode.Result);
    }

    private async void showLeaderboardButton_Click(object? sender, EventArgs e)
    {
        await SetDisplayModeAsync(DisplayMode.Leaderboard);
    }

    private async Task SetDisplayModeAsync(DisplayMode mode)
    {
        await RunUiOperationAsync(async () =>
        {
            var eventId = GetEventId();
            using var request = CreateHostRequest(
                HttpMethod.Put,
                $"api/v1/events/{eventId:D}/display/mode");
            request.Content = JsonContent.Create(new { mode });
            using var response = await httpClient.SendAsync(request);
            await EnsureSuccessAsync(response);
            var state = await response.Content.ReadFromJsonAsync<DisplayStateView>()
                ?? throw new InvalidOperationException("Server 未回傳 Display 狀態。");
            displayModeValueLabel.Text = $"目前畫面：{state.Mode}";
        });
    }

    private async Task LoadDisplayStateAsync(Guid eventId)
    {
        using var response = await httpClient.GetAsync(
            new Uri(GetServerUri(), $"api/v1/events/{eventId:D}/display"));
        await EnsureSuccessAsync(response);
        var state = await response.Content.ReadFromJsonAsync<DisplayStateView>()
            ?? throw new InvalidOperationException("Server 未回傳 Display 狀態。");
        displayModeValueLabel.Text = $"目前畫面：{state.Mode}";
    }

    private void UpdateDisplayModeThreadSafe(DisplayMode mode)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdateDisplayModeThreadSafe(mode));
            return;
        }

        displayModeValueLabel.Text = $"目前畫面：{mode}";
    }

    private async Task ExecuteSessionCommandAsync(string command)
    {
        await RunUiOperationAsync(async () =>
        {
            if (!currentSessionId.HasValue)
            {
                throw new InvalidOperationException("目前沒有題目場次。");
            }

            var eventId = GetEventId();
            using var request = CreateHostRequest(
                HttpMethod.Post,
                $"api/v1/events/{eventId:D}/quiz/sessions/{currentSessionId.Value:D}/{command}");
            request.Content = JsonContent.Create(new { });
            using var response = await httpClient.SendAsync(request);
            await EnsureSuccessAsync(response);
            var state = await response.Content.ReadFromJsonAsync<QuizStateView>()
                ?? throw new InvalidOperationException("Server 未回傳 Quiz 狀態。");
            ApplyQuizState(state);
        });
    }

    private async Task LoadQuizStateAsync(Guid eventId)
    {
        using var request = CreateHostRequest(
            HttpMethod.Get,
            $"api/v1/events/{eventId:D}/quiz/current");
        using var response = await httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
        var state = await response.Content.ReadFromJsonAsync<QuizStateView>()
            ?? throw new InvalidOperationException("Server 未回傳 Quiz 狀態。");
        ApplyQuizState(state);
        if (state.State == QuizState.Revealed && state.SessionId.HasValue)
        {
            await LoadQuizResultsAsync(eventId, state.SessionId.Value);
        }
    }

    private void RefreshQuizStateThreadSafe(Guid eventId)
    {
        if (IsDisposed)
        {
            return;
        }

        BeginInvoke(async () =>
        {
            try
            {
                await LoadQuizStateAsync(eventId);
            }
            catch (Exception exception)
            {
                statusLabel.Text = $"Quiz 狀態同步失敗：{exception.Message}";
            }
        });
    }

    private void UpdateQuizProgressThreadSafe(QuestionProgressNotification progress)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpdateQuizProgressThreadSafe(progress));
            return;
        }

        if (currentSessionId == progress.SessionId)
        {
            quizProgressLabel.Text =
                $"已作答 {progress.AnsweredCount} / {progress.ParticipantCount}　在線 {progress.OnlineCount}";
        }
    }

    private void RefreshQuizResultsThreadSafe(Guid eventId)
    {
        if (IsDisposed || !currentSessionId.HasValue)
        {
            return;
        }

        var sessionId = currentSessionId.Value;
        BeginInvoke(async () =>
        {
            try
            {
                await LoadQuizResultsAsync(eventId, sessionId);
            }
            catch (Exception exception)
            {
                statusLabel.Text = $"Quiz 結果同步失敗：{exception.Message}";
            }
        });
    }

    private async Task LoadQuizResultsAsync(Guid eventId, Guid sessionId)
    {
        using var statisticsRequest = CreateHostRequest(
            HttpMethod.Get,
            $"api/v1/events/{eventId:D}/quiz/sessions/{sessionId:D}/stats");
        using var statisticsResponse = await httpClient.SendAsync(statisticsRequest);
        await EnsureSuccessAsync(statisticsResponse);
        var statistics = await statisticsResponse.Content.ReadFromJsonAsync<QuizStatisticsView>()
            ?? throw new InvalidOperationException("Server 未回傳 Quiz 統計。");

        using var leaderboardRequest = CreateHostRequest(
            HttpMethod.Get,
            $"api/v1/events/{eventId:D}/quiz/leaderboard?top=10");
        using var leaderboardResponse = await httpClient.SendAsync(leaderboardRequest);
        await EnsureSuccessAsync(leaderboardResponse);
        var leaderboard = await leaderboardResponse.Content.ReadFromJsonAsync<QuizLeaderboardView>()
            ?? throw new InvalidOperationException("Server 未回傳排行榜。");

        quizResultLabel.Text =
            $"結果：答對 {statistics.CorrectCount}　答錯 {statistics.IncorrectCount}　未作答 {statistics.NoAnswerCount}　正確率 {statistics.CorrectRate:P0}";
        quizLeaderboardGrid.Rows.Clear();
        foreach (var entry in leaderboard.Entries)
        {
            quizLeaderboardGrid.Rows.Add(
                entry.Rank,
                entry.DisplayName,
                entry.TotalScore,
                entry.CorrectCount,
                entry.AnsweredCount);
        }
    }

    private void ApplyQuizState(QuizStateView state)
    {
        currentSessionId = state.SessionId;
        selectedQuestionId = state.QuestionId ?? selectedQuestionId;
        currentQuestionLabel.Text = $"目前題目：{state.QuestionText ?? "等待建立或開始題目"}";
        quizStateLabel.Text = $"狀態：{state.State}";
        quizProgressLabel.Text = $"已作答 {state.AnsweredCount} / {state.ParticipantCount}　在線 {state.OnlineCount}";
        if (state.State != QuizState.Revealed)
        {
            quizResultLabel.Text = "結果：等待公布答案";
            quizLeaderboardGrid.Rows.Clear();
        }
        UpdateQuizButtons(state.State);
        ScheduleDeadlineRefresh(state);
    }

    private void ScheduleDeadlineRefresh(QuizStateView state)
    {
        quizDeadlineRefreshCancellation?.Cancel();
        quizDeadlineRefreshCancellation?.Dispose();
        quizDeadlineRefreshCancellation = null;
        if (state.State != QuizState.Open || !state.AnswerDeadlineUtc.HasValue)
        {
            return;
        }

        quizDeadlineRefreshCancellation = new CancellationTokenSource();
        _ = RefreshAtDeadlineAsync(
            state.AnswerDeadlineUtc.Value,
            quizDeadlineRefreshCancellation.Token);
    }

    private async Task RefreshAtDeadlineAsync(DateTimeOffset deadlineUtc, CancellationToken cancellationToken)
    {
        try
        {
            var delay = deadlineUtc - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken);
            }

            if (!cancellationToken.IsCancellationRequested && !IsDisposed)
            {
                BeginInvoke(async () => await LoadQuizStateAsync(GetEventId()));
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void UpdateQuizButtons(QuizState state)
    {
        startQuestionButton.Enabled = selectedQuestionId.HasValue && state != QuizState.Open;
        closeQuestionButton.Enabled = currentSessionId.HasValue && state == QuizState.Open;
        revealAnswerButton.Enabled = currentSessionId.HasValue && state == QuizState.Closed;
    }

    private async Task LoadParticipantsAsync(Guid eventId)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(GetServerUri(), $"api/v1/events/{eventId:D}/participants"));
        request.Headers.Add("X-Host-Token", hostTokenTextBox.Text);
        using var response = await httpClient.SendAsync(request);
        await EnsureSuccessAsync(response);
        var participants = await response.Content.ReadFromJsonAsync<List<ParticipantView>>() ?? [];

        participantGrid.Rows.Clear();
        foreach (var participant in participants)
        {
            UpsertParticipant(participant);
        }

        RefreshOnlineCount();
    }

    private void UpsertParticipantThreadSafe(ParticipantView participant)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => UpsertParticipant(participant));
            return;
        }

        UpsertParticipant(participant);
    }

    private void UpsertParticipant(ParticipantView participant)
    {
        DataGridViewRow? existingRow = null;
        foreach (DataGridViewRow row in participantGrid.Rows)
        {
            if (row.Tag is Guid participantId && participantId == participant.Id)
            {
                existingRow = row;
                break;
            }
        }

        var targetRow = existingRow ?? participantGrid.Rows[participantGrid.Rows.Add()];
        targetRow.Tag = participant.Id;
        targetRow.Cells[nameColumn.Index].Value = participant.DisplayName;
        targetRow.Cells[employeeNumberColumn.Index].Value = participant.EmployeeNumber;
        targetRow.Cells[departmentColumn.Index].Value = participant.Department;
        targetRow.Cells[tableNumberColumn.Index].Value = participant.TableNumber;
        targetRow.Cells[onlineColumn.Index].Value = participant.IsOnline ? "在線" : "離線";
        targetRow.Cells[scoreColumn.Index].Value = participant.Score;
        RefreshOnlineCount();
    }

    private void RefreshOnlineCount()
    {
        var onlineCount = participantGrid.Rows
            .Cast<DataGridViewRow>()
            .Count(row => string.Equals(row.Cells[onlineColumn.Index].Value?.ToString(), "在線", StringComparison.Ordinal));
        onlineCountLabel.Text = $"在線 {onlineCount} / 總計 {participantGrid.Rows.Count}";
    }

    private Uri GetServerUri()
    {
        if (!Uri.TryCreate(serverUrlTextBox.Text.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("Server URL 必須是有效的 HTTP 或 HTTPS 網址。");
        }

        return new Uri(uri.AbsoluteUri.TrimEnd('/') + "/");
    }

    private Guid GetEventId()
    {
        return Guid.TryParse(eventIdTextBox.Text, out var eventId)
            ? eventId
            : throw new InvalidOperationException("活動 ID 格式不正確。");
    }

    private HttpRequestMessage CreateHostRequest(HttpMethod method, string relativeUri)
    {
        var request = new HttpRequestMessage(method, new Uri(GetServerUri(), relativeUri));
        request.Headers.Add("X-Host-Token", hostTokenTextBox.Text);
        return request;
    }

    private async Task RunUiOperationAsync(Func<Task> operation)
    {
        createEventButton.Enabled = false;
        connectButton.Enabled = false;

        try
        {
            await operation();
        }
        catch (Exception exception)
        {
            statusLabel.Text = "操作失敗";
            MessageBox.Show(this, exception.Message, "EventHub", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            createEventButton.Enabled = true;
            connectButton.Enabled = true;
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var problem = await response.Content.ReadFromJsonAsync<ProblemResponse>();
        throw new InvalidOperationException(problem?.Detail ?? $"Server 回傳 {(int)response.StatusCode}。");
    }

    private Task UpdateStatusAsync(string message)
    {
        UpdateStatusThreadSafe(message);
        return Task.CompletedTask;
    }

    private void UpdateStatusThreadSafe(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => statusLabel.Text = message);
            return;
        }

        statusLabel.Text = message;
    }

    private async void HostDashboardForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        quizDeadlineRefreshCancellation?.Cancel();
        quizDeadlineRefreshCancellation?.Dispose();
        await DisconnectHubAsync();
        joinQrCodePictureBox.Image?.Dispose();
        httpClient.Dispose();
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

    private sealed record CreateEventRequest(string Name, DateTime EventDateUtc);

    private sealed record CreateEventResult(
        EventView Event,
        string HostToken,
        EventJoinInfoView JoinInfo);

    private sealed record EventView(Guid Id, string Name);

    private sealed record EventJoinInfoView(
        Guid EventId,
        string EventName,
        string JoinCode,
        string JoinUrl,
        bool IsJoinOpen,
        bool IsLoopback);

    private sealed record ParticipantView(
        Guid Id,
        string DisplayName,
        string? EmployeeNumber,
        string? Department,
        string? TableNumber,
        bool IsOnline,
        int Score);

    private sealed record ProblemResponse(string? Detail);

    private sealed record CreateQuestionRequest(
        string QuestionText,
        IReadOnlyCollection<string> Options,
        int CorrectOptionIndex,
        int AnswerDurationSeconds);

    private sealed record QuestionView(Guid Id, string Text);

    private sealed record QuizStateView(
        QuizState State,
        Guid? SessionId,
        Guid? QuestionId,
        string? QuestionText,
        int AnsweredCount,
        int ParticipantCount,
        int OnlineCount,
        DateTimeOffset? AnswerDeadlineUtc);

    private sealed record QuestionStartedNotification(Guid SessionId);

    private sealed record QuestionProgressNotification(
        Guid SessionId,
        int AnsweredCount,
        int ParticipantCount,
        int OnlineCount);

    private sealed record QuestionClosedNotification(Guid SessionId);

    private sealed record AnswerRevealedNotification(Guid SessionId);

    private sealed record LeaderboardUpdatedNotification(Guid SessionId);

    private sealed record DisplayModeChangedNotification(Guid EventId, DisplayMode Mode);

    private sealed record DisplayStateView(DisplayMode Mode);

    private sealed record QuizStatisticsView(
        int CorrectCount,
        int IncorrectCount,
        int NoAnswerCount,
        decimal CorrectRate);

    private sealed record QuizLeaderboardView(IReadOnlyList<QuizLeaderboardEntryView> Entries);

    private sealed record QuizLeaderboardEntryView(
        int Rank,
        string DisplayName,
        int TotalScore,
        int CorrectCount,
        int AnsweredCount);

    private enum QuizState
    {
        Waiting,
        Open,
        Closed,
        Revealed
    }

    private enum DisplayMode
    {
        Waiting,
        Question,
        Result,
        Leaderboard
    }
}
