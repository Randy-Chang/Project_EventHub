namespace EventHub.Host.Views;

internal partial class LiveMonitorView : UserControl
{
    private DisplayMode currentDisplayMode;
    private DateTimeOffset? answerDeadlineUtc;

    public LiveMonitorView()
    {
        InitializeComponent();
    }

    public event Action<DisplayMode>? DisplayModeRequested;

    public void Render(
        QuizQuestionMode? mode,
        string questionPosition,
        QuizStateView state,
        DisplayMode displayMode)
    {
        activityValueLabel.Text = "Quiz";
        questionValueLabel.Text = questionPosition;
        modeValueLabel.Text = mode?.ToString().ToUpperInvariant() ?? "—";
        stateValueLabel.Text = mode.HasValue ? state.State.ToString().ToUpperInvariant() : "NO QUIZ";
        answeredValueLabel.Text = mode.HasValue ? $"{state.AnsweredCount} / {state.ParticipantCount}" : "—";
        onlineValueLabel.Text = state.OnlineCount.ToString();
        answerDeadlineUtc = state.State == QuizState.Open ? state.AnswerDeadlineUtc : null;
        RenderDisplayMode(displayMode);
        UpdateCountdown();
    }

    public void RenderDisplayMode(DisplayMode mode)
    {
        currentDisplayMode = mode;
        displayModeValueLabel.Text = ToDisplayLabel(mode);
        waitingButton.Enabled = mode != DisplayMode.Waiting;
        liveButton.Enabled = mode != DisplayMode.Question;
        resultButton.Enabled = mode != DisplayMode.Result;
        rankingButton.Enabled = mode != DisplayMode.Leaderboard;
    }

    public void SetDisplayBusy(bool isBusy)
    {
        waitingButton.Enabled = !isBusy && currentDisplayMode != DisplayMode.Waiting;
        liveButton.Enabled = !isBusy && currentDisplayMode != DisplayMode.Question;
        resultButton.Enabled = !isBusy && currentDisplayMode != DisplayMode.Result;
        rankingButton.Enabled = !isBusy && currentDisplayMode != DisplayMode.Leaderboard;
    }

    public void SetStatus(string message, bool isError = false)
    {
        displayStatusLabel.Text = message;
        displayStatusLabel.ForeColor = isError ? Color.Firebrick : Color.DarkGreen;
    }

    private static string ToDisplayLabel(DisplayMode mode) => mode switch
    {
        DisplayMode.Question => "LIVE",
        DisplayMode.Leaderboard => "RANKING",
        _ => mode.ToString().ToUpperInvariant()
    };

    private void waitingButton_Click(object? sender, EventArgs e) => DisplayModeRequested?.Invoke(DisplayMode.Waiting);
    private void liveButton_Click(object? sender, EventArgs e) => DisplayModeRequested?.Invoke(DisplayMode.Question);
    private void resultButton_Click(object? sender, EventArgs e) => DisplayModeRequested?.Invoke(DisplayMode.Result);
    private void rankingButton_Click(object? sender, EventArgs e) => DisplayModeRequested?.Invoke(DisplayMode.Leaderboard);
    private void countdownTimer_Tick(object? sender, EventArgs e) => UpdateCountdown();

    private void UpdateCountdown()
    {
        if (!answerDeadlineUtc.HasValue)
        {
            timeValueLabel.Text = "--:--";
            return;
        }

        var remaining = answerDeadlineUtc.Value - DateTimeOffset.UtcNow;
        var seconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));
        timeValueLabel.Text = $"{seconds / 60:00}:{seconds % 60:00}";
    }
}
