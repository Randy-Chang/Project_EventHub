namespace EventHub.Host.Views;

internal partial class DisplayControlView : UserControl
{
    private DisplayMode currentMode;

    public DisplayControlView()
    {
        InitializeComponent();
    }

    public event Action<DisplayMode>? DisplayModeRequested;

    public void Render(DisplayMode mode)
    {
        currentMode = mode;
        currentModeValueLabel.Text = mode.ToString();
        waitingButton.Enabled = mode != DisplayMode.Waiting;
        questionButton.Enabled = mode != DisplayMode.Question;
        resultButton.Enabled = mode != DisplayMode.Result;
        leaderboardButton.Enabled = mode != DisplayMode.Leaderboard;
    }

    public void SetStatus(string message, bool isError = false)
    {
        statusLabel.Text = message;
        statusLabel.ForeColor = isError ? Color.Firebrick : Color.DarkGreen;
    }

    public void SetBusy(bool isBusy)
    {
        waitingButton.Enabled = !isBusy && currentMode != DisplayMode.Waiting;
        questionButton.Enabled = !isBusy && currentMode != DisplayMode.Question;
        resultButton.Enabled = !isBusy && currentMode != DisplayMode.Result;
        leaderboardButton.Enabled = !isBusy && currentMode != DisplayMode.Leaderboard;
    }

    private void waitingButton_Click(object? sender, EventArgs e) => DisplayModeRequested?.Invoke(DisplayMode.Waiting);
    private void questionButton_Click(object? sender, EventArgs e) => DisplayModeRequested?.Invoke(DisplayMode.Question);
    private void resultButton_Click(object? sender, EventArgs e) => DisplayModeRequested?.Invoke(DisplayMode.Result);
    private void leaderboardButton_Click(object? sender, EventArgs e) => DisplayModeRequested?.Invoke(DisplayMode.Leaderboard);
}
