using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EventHub.Host.Views;

internal enum OperationMessageKind
{
    Information,
    Success,
    Warning,
    Error
}

internal partial class EventManagementView : UserControl
{
    private const int EventHubServerPort = 5000;
    private string currentJoinCode = string.Empty;
    private bool isBusy;
    private bool isJoinPolicyAvailable;

    public EventManagementView()
    {
        InitializeComponent();
    }

    public event EventHandler? CreateEventRequested;
    public event EventHandler? ConnectRequested;
    public event EventHandler? ResumeRecentRequested;
    public event EventHandler? ForgetRecentRequested;
    public event EventHandler? ToggleJoinPolicyRequested;
    public event EventHandler? RefreshLanAddressesRequested;
    public event EventHandler? TestConnectionRequested;
    public event EventHandler? InstallFirewallRuleRequested;

    public string ServerUrl => serverUrlTextBox.Text.Trim();
    public string PublicServerUrl => lanAddressComboBox.SelectedItem is LanAddressOption selected
        ? selected.CreatePublicBaseUrl(EventHubServerPort)
        : throw new InvalidOperationException("找不到可用的 LAN IPv4，請確認網路線或 Wi-Fi 已連線。");
    public string EventName => eventNameTextBox.Text.Trim();
    public DateTime EventDateUtc => eventDatePicker.Value.ToUniversalTime();
    public string HostToken => hostTokenTextBox.Text;

    public Guid EventId => Guid.TryParse(eventIdTextBox.Text, out var eventId)
        ? eventId
        : throw new InvalidOperationException("活動 ID 格式不正確。");

    public void RenderLanAddresses(IReadOnlyList<LanAddressOption> addresses)
    {
        var previousAddress = (lanAddressComboBox.SelectedItem as LanAddressOption)?.Address;
        lanAddressComboBox.BeginUpdate();
        try
        {
            lanAddressComboBox.Items.Clear();
            lanAddressComboBox.Items.AddRange(addresses.Cast<object>().ToArray());
            var selectedIndex = previousAddress is null
                ? -1
                : addresses.ToList().FindIndex(item => item.Address.Equals(previousAddress));
            if (selectedIndex < 0)
            {
                selectedIndex = addresses.ToList().FindIndex(item => item.IsPreferred);
            }

            if (selectedIndex < 0 && addresses.Count > 0)
            {
                selectedIndex = 0;
            }

            lanAddressComboBox.SelectedIndex = selectedIndex;
        }
        finally
        {
            lanAddressComboBox.EndUpdate();
        }

        RenderSelectedNetwork();
    }

    public void SetEventCredential(Guid eventId, string hostToken)
    {
        eventIdTextBox.Text = eventId.ToString();
        hostTokenTextBox.Text = hostToken;
        currentEventIdValueLabel.Text = ShortenEventId(eventId);
        currentEventDateValueLabel.Text = eventDatePicker.Value.ToString("yyyy/MM/dd HH:mm");
        eventIdToolTip.SetToolTip(currentEventIdValueLabel, eventId.ToString());
    }

    public void SetResumeContext(RecentHostSession session)
    {
        serverUrlTextBox.Text = session.ServerUrl;
        SetEventCredential(session.EventId, session.HostToken);
    }

    public void RenderRecentSession(RecentHostSession? session)
    {
        recentSessionLabel.Text = session is null
            ? "沒有可繼續主持的最近活動"
            : $"最近活動：{session.EventName}　上次連線 {session.LastConnectedAtUtc.ToLocalTime():yyyy/MM/dd HH:mm}";
        resumeRecentButton.Visible = session is not null;
        forgetRecentButton.Visible = session is not null;
    }

    public void SetJoinPolicyAvailability(bool enabled)
    {
        isJoinPolicyAvailable = enabled;
        toggleJoinPolicyButton.Enabled = enabled && !isBusy;
    }

    public void RenderJoinInfo(EventJoinInfoView joinInfo, Image qrCode)
    {
        eventIdTextBox.Text = joinInfo.EventId.ToString();
        currentEventNameValueLabel.Text = joinInfo.EventName;
        eventSetupTitleLabel.Text = "STEP 2　建立或連線活動　✓ 已完成";
        currentEventIdValueLabel.Text = ShortenEventId(joinInfo.EventId);
        eventIdToolTip.SetToolTip(currentEventIdValueLabel, joinInfo.EventId.ToString());
        currentJoinCode = joinInfo.JoinCode;
        joinPolicyValueLabel.Text = joinInfo.IsJoinOpen ? "開放中" : "已關閉";
        joinPolicyValueLabel.ForeColor = joinInfo.IsJoinOpen ? Color.DarkGreen : Color.Firebrick;
        toggleJoinPolicyButton.Text = joinInfo.IsJoinOpen ? "關閉報到" : "開放報到";
        joinCodeValueLabel.Text = string.Join(" ", joinInfo.JoinCode.ToCharArray());
        mobileJoinTitleLabel.Text = "STEP 4　手機加入　✓ 加入資訊已準備";
        joinUrlTextBox.Text = joinInfo.JoinUrl;
        joinUrlWarningLabel.Text = !joinInfo.IsJoinOpen
            ? "目前尚未開放加入；請先執行「開放參與者加入」。"
            : joinInfo.IsLoopback
                ? "警告：目前使用 localhost，其他手機無法連線。"
                : "同一 Wi-Fi 的手機可掃描 QR Code 加入。";
        joinUrlWarningLabel.ForeColor = !joinInfo.IsJoinOpen || joinInfo.IsLoopback
            ? Color.Firebrick
            : Color.DarkGreen;
        var previous = joinQrCodePictureBox.Image;
        joinQrCodePictureBox.Image = qrCode;
        previous?.Dispose();
    }

    public void RenderParticipants(IReadOnlyCollection<ParticipantView> participants)
    {
        participantGrid.Rows.Clear();
        foreach (var participant in participants)
        {
            UpsertParticipant(participant);
        }

        UpdateParticipantPresentation();
    }

    public void ResetCurrentContext()
    {
        currentEventNameValueLabel.Text = "尚未建立或連線";
        currentEventDateValueLabel.Text = "—";
        currentEventIdValueLabel.Text = "—";
        currentEventStatusValueLabel.Text = "未連線";
        joinPolicyValueLabel.Text = "尚未開放";
        joinPolicyValueLabel.ForeColor = SystemColors.ControlText;
        toggleJoinPolicyButton.Text = "開放報到";
        isJoinPolicyAvailable = false;
        toggleJoinPolicyButton.Enabled = false;
        eventSetupTitleLabel.Text = "STEP 2　建立或連線活動　○ 尚未完成";
        mobileJoinTitleLabel.Text = "STEP 4　手機加入　○ 尚未準備";
        eventIdToolTip.SetToolTip(currentEventIdValueLabel, null);
        currentJoinCode = string.Empty;
        joinCodeValueLabel.Text = "— — — — — —";
        joinUrlTextBox.Clear();
        joinUrlWarningLabel.Text = "建立活動後顯示 QR Code。";
        joinUrlWarningLabel.ForeColor = SystemColors.ControlText;
        var previous = joinQrCodePictureBox.Image;
        joinQrCodePictureBox.Image = null;
        previous?.Dispose();
        participantGrid.Rows.Clear();
        UpdateParticipantPresentation();
    }

    public void UpsertParticipant(ParticipantView participant)
    {
        var row = participantGrid.Rows.Cast<DataGridViewRow>()
            .FirstOrDefault(item => item.Tag is Guid id && id == participant.Id)
            ?? participantGrid.Rows[participantGrid.Rows.Add()];
        row.Tag = participant.Id;
        row.SetValues(
            participant.DisplayName,
            participant.EmployeeNumber,
            participant.Department,
            participant.TableNumber,
            participant.IsOnline ? "在線" : "離線",
            participant.Score);
        UpdateParticipantPresentation();
    }

    public void RenderConnectionState(HostConnectionState state)
    {
        var (text, color) = state switch
        {
            HostConnectionState.Connected => ("已連線", Color.DarkGreen),
            HostConnectionState.Connecting => ("連線中…", Color.DarkOrange),
            HostConnectionState.Reconnecting => ("重新連線中…", Color.DarkOrange),
            _ => ("未連線", Color.Firebrick)
        };
        currentEventStatusValueLabel.Text = text;
        currentEventStatusValueLabel.ForeColor = color;
        currentEventTitleLabel.Text = state == HostConnectionState.Connected
            ? "STEP 3　目前活動　✓ 已連線"
            : $"STEP 3　目前活動　○ {text}";
        serverStatusValueLabel.Text = text;
        serverStatusValueLabel.ForeColor = color;
    }

    public void RenderFirewallReady()
    {
        firewallStatusValueLabel.Text = "已就緒";
        firewallStatusValueLabel.ForeColor = Color.DarkGreen;
    }

    public void RenderNetworkReady()
    {
        networkStatusTitleLabel.Text = "STEP 1　活動網路　✓ 網路已準備完成";
    }

    public void SetStatus(string message, bool isError = false)
    {
        SetStatus(message, isError ? OperationMessageKind.Error : OperationMessageKind.Success);
    }

    public void SetStatus(string message, OperationMessageKind kind)
    {
        statusLabel.Text = message;
        statusLabel.ForeColor = kind switch
        {
            OperationMessageKind.Error => Color.Firebrick,
            OperationMessageKind.Warning => Color.DarkOrange,
            OperationMessageKind.Success => Color.DarkGreen,
            _ => Color.FromArgb(35, 67, 92)
        };
        operationMessagePanel.BackColor = kind switch
        {
            OperationMessageKind.Error => Color.FromArgb(255, 241, 241),
            OperationMessageKind.Warning => Color.FromArgb(255, 248, 230),
            OperationMessageKind.Success => Color.FromArgb(241, 248, 245),
            _ => Color.FromArgb(240, 246, 252)
        };
    }

    public void SetBusy(bool isBusy)
    {
        this.isBusy = isBusy;
        createEventButton.Enabled = !isBusy;
        connectButton.Enabled = !isBusy;
        refreshLanAddressesButton.Enabled = !isBusy;
        testConnectionButton.Enabled = !isBusy;
        installFirewallRuleButton.Enabled = !isBusy;
        toggleJoinPolicyButton.Enabled = !isBusy && isJoinPolicyAvailable;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            joinQrCodePictureBox.Image?.Dispose();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void createEventButton_Click(object? sender, EventArgs e) => CreateEventRequested?.Invoke(this, EventArgs.Empty);
    private void connectButton_Click(object? sender, EventArgs e) => ConnectRequested?.Invoke(this, EventArgs.Empty);
    private void resumeRecentButton_Click(object? sender, EventArgs e) =>
        ResumeRecentRequested?.Invoke(this, EventArgs.Empty);
    private void forgetRecentButton_Click(object? sender, EventArgs e) =>
        ForgetRecentRequested?.Invoke(this, EventArgs.Empty);
    private void toggleJoinPolicyButton_Click(object? sender, EventArgs e) =>
        ToggleJoinPolicyRequested?.Invoke(this, EventArgs.Empty);
    private void refreshLanAddressesButton_Click(object? sender, EventArgs e) =>
        RefreshLanAddressesRequested?.Invoke(this, EventArgs.Empty);
    private void testConnectionButton_Click(object? sender, EventArgs e) =>
        TestConnectionRequested?.Invoke(this, EventArgs.Empty);
    private void installFirewallRuleButton_Click(object? sender, EventArgs e) =>
        InstallFirewallRuleRequested?.Invoke(this, EventArgs.Empty);

    private void lanAddressComboBox_SelectedIndexChanged(object? sender, EventArgs e) => RenderSelectedNetwork();

    private void copyEventIdButton_Click(object? sender, EventArgs e) =>
        CopyText(eventIdTextBox.Text, "Event ID");

    private void copyJoinCodeButton_Click(object? sender, EventArgs e) =>
        CopyText(currentJoinCode, "Join Code");

    private void copyJoinUrlButton_Click(object? sender, EventArgs e) =>
        CopyText(joinUrlTextBox.Text, "加入網址");

    private void CopyText(string value, string description)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"目前沒有可複製的 {description}。");
            }

            Clipboard.SetText(value);
            SetStatus($"✓ {description} 已複製。", OperationMessageKind.Success);
        }
        catch (Exception exception) when (exception is ExternalException or InvalidOperationException)
        {
            Trace.TraceError($"Clipboard copy failed for {description}: {exception}");
            SetStatus($"✕ 無法複製到剪貼簿：{exception.Message}", OperationMessageKind.Error);
        }
    }

    private void RenderSelectedNetwork()
    {
        if (lanAddressComboBox.SelectedItem is LanAddressOption selected)
        {
            networkReadyValueLabel.Text = selected.ToString();
            networkReadyValueLabel.ForeColor = Color.DarkGreen;
            networkStatusTitleLabel.Text = "STEP 1　活動網路　✓ 已選擇網路";
            return;
        }

        networkReadyValueLabel.Text = "找不到可用的 LAN IPv4";
        networkReadyValueLabel.ForeColor = Color.Firebrick;
        networkStatusTitleLabel.Text = "STEP 1　活動網路　⚠ 尚未準備完成";
    }

    private void UpdateParticipantPresentation()
    {
        var count = participantGrid.Rows.Count;
        participantsTitleLabel.Text = $"STEP 5　參與者　{count} 位已加入";
        participantEmptyLabel.Visible = count == 0;
    }

    private static string ShortenEventId(Guid eventId)
    {
        var value = eventId.ToString();
        return $"{value[..8]}…{value[^4..]}";
    }
}
