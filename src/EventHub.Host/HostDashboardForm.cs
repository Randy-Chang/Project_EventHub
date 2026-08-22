using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace EventHub.Host;

public partial class HostDashboardForm : Form
{
    private readonly HttpClient httpClient = new();
    private HubConnection? hubConnection;

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
        hubConnection.Reconnecting += _ => UpdateStatusAsync("SignalR 重新連線中…");
        hubConnection.Reconnected += _ =>
        {
            UpdateStatusThreadSafe("SignalR 已重新連線");
            BeginInvoke(async () => await LoadParticipantsAsync(eventId));
            return Task.CompletedTask;
        };
        hubConnection.Closed += _ => UpdateStatusAsync("SignalR 已離線");

        await hubConnection.StartAsync();
        await LoadParticipantsAsync(eventId);
        statusLabel.Text = "已連線，正在監看參與者";
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
        await DisconnectHubAsync();
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

    private sealed record CreateEventResult(EventView Event, string HostToken);

    private sealed record EventView(Guid Id, string Name);

    private sealed record ParticipantView(
        Guid Id,
        string DisplayName,
        string? EmployeeNumber,
        string? Department,
        string? TableNumber,
        bool IsOnline,
        int Score);

    private sealed record ProblemResponse(string? Detail);
}
