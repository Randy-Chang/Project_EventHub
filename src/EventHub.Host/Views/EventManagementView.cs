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
    public EventManagementView()
    {
        InitializeComponent();
    }

    public event EventHandler? CreateEventRequested;
    public event EventHandler? ConnectRequested;

    public string ServerUrl => serverUrlTextBox.Text.Trim();
    public string EventName => eventNameTextBox.Text.Trim();
    public DateTime EventDateUtc => eventDatePicker.Value.ToUniversalTime();
    public string HostToken => hostTokenTextBox.Text;

    public Guid EventId => Guid.TryParse(eventIdTextBox.Text, out var eventId)
        ? eventId
        : throw new InvalidOperationException("活動 ID 格式不正確。");

    public void SetEventCredential(Guid eventId, string hostToken)
    {
        eventIdTextBox.Text = eventId.ToString();
        hostTokenTextBox.Text = hostToken;
    }

    public void RenderJoinInfo(EventJoinInfoView joinInfo, Image qrCode)
    {
        eventInfoValueLabel.Text = $"{joinInfo.EventName}　Join Code：{joinInfo.JoinCode}";
        joinUrlTextBox.Text = joinInfo.JoinUrl;
        joinUrlWarningLabel.Text = joinInfo.IsLoopback
            ? "警告：目前使用 localhost，其他手機無法連線。"
            : "同一 Wi-Fi 的手機可掃描 QR Code 加入。";
        joinUrlWarningLabel.ForeColor = joinInfo.IsLoopback ? Color.Firebrick : Color.DarkGreen;
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
    }

    public void ResetCurrentContext()
    {
        eventInfoValueLabel.Text = "尚未建立或連線";
        joinUrlTextBox.Clear();
        joinUrlWarningLabel.Text = "建立活動後顯示 QR Code。";
        joinUrlWarningLabel.ForeColor = SystemColors.ControlText;
        var previous = joinQrCodePictureBox.Image;
        joinQrCodePictureBox.Image = null;
        previous?.Dispose();
        participantGrid.Rows.Clear();
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
        createEventButton.Enabled = !isBusy;
        connectButton.Enabled = !isBusy;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            joinQrCodePictureBox.Image?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void createEventButton_Click(object? sender, EventArgs e) => CreateEventRequested?.Invoke(this, EventArgs.Empty);
    private void connectButton_Click(object? sender, EventArgs e) => ConnectRequested?.Invoke(this, EventArgs.Empty);

    private void copyJoinUrlButton_Click(object? sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(joinUrlTextBox.Text))
            {
                throw new InvalidOperationException("目前沒有可複製的加入網址。");
            }

            Clipboard.SetText(joinUrlTextBox.Text);
            SetStatus("加入網址已複製。");
        }
        catch (Exception exception) when (exception is ExternalException or InvalidOperationException)
        {
            SetStatus($"複製失敗：{exception.Message}", true);
        }
    }
}
