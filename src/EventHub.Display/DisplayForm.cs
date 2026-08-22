using EventHub.Presentation;

namespace EventHub.Display;

public partial class DisplayForm : Form
{
    private readonly DisplayClientOptions options;
    private readonly EventHubDisplayClient displayClient = new();
    private readonly QrCodePngGenerator qrCodeGenerator = new();
    private readonly System.Windows.Forms.Timer countdownTimer = new() { Interval = 250 };
    private Guid? currentSessionId;
    private DateTimeOffset? answerDeadlineUtc;
    private TimeSpan serverClockOffset;
    private bool isFullscreen;
    private Rectangle previousBounds;
    private FormBorderStyle previousBorderStyle;

    public DisplayForm(DisplayClientOptions options)
    {
        this.options = options;
        InitializeComponent();
        countdownTimer.Tick += countdownTimer_Tick;
        displayClient.StateReceived += state => RunOnUiThread(() => RenderState(state));
        displayClient.QuestionProgressReceived += progress =>
            RunOnUiThread(() => UpdateQuestionProgress(progress));
        displayClient.ParticipantCountReceived += count =>
            RunOnUiThread(() => UpdateParticipantCount(count.ParticipantCount));
        displayClient.ConnectionStatusChanged += (message, isError) =>
            RunOnUiThread(() => UpdateConnectionStatus(message, isError));
    }

    private void DisplayForm_Shown(object? sender, EventArgs e)
    {
        serverUrlTextBox.Text = options.ServerApiBaseUrl;
        eventIdTextBox.Text = options.EventId;
        fullscreenCheckBox.Checked = options.StartFullscreen;
        screenComboBox.Items.Clear();
        foreach (var screen in Screen.AllScreens)
        {
            screenComboBox.Items.Add(
                $"{screen.DeviceName} · {screen.Bounds.Width}×{screen.Bounds.Height}" +
                (screen.Primary ? " · Primary" : " · Secondary"));
        }

        var preferredIndex = options.DisplayScreenIndex;
        if (preferredIndex < 0 || preferredIndex >= Screen.AllScreens.Length)
        {
            preferredIndex = Array.FindIndex(Screen.AllScreens, screen => !screen.Primary);
        }

        screenComboBox.SelectedIndex = preferredIndex >= 0 ? preferredIndex : 0;
        if (Guid.TryParse(options.EventId, out _))
        {
            _ = ConnectAsync();
        }
    }

    private async void connectButton_Click(object? sender, EventArgs e)
    {
        await ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        connectButton.Enabled = false;
        try
        {
            if (!Guid.TryParse(eventIdTextBox.Text, out var eventId))
            {
                throw new InvalidOperationException("請輸入正確的 Event ID。");
            }

            UpdateConnectionStatus("正在連線…", false);
            await displayClient.ConnectAsync(
                serverUrlTextBox.Text,
                eventId,
                options.LeaderboardTop,
                CancellationToken.None);
            if (fullscreenCheckBox.Checked)
            {
                EnterFullscreen();
            }
        }
        catch (Exception exception)
        {
            UpdateConnectionStatus($"無法連線：{exception.Message}", true);
            setupPanel.Visible = true;
        }
        finally
        {
            connectButton.Enabled = true;
        }
    }

    private void RenderState(CurrentDisplayStateDto state)
    {
        serverClockOffset = state.ServerTimeUtc - DateTimeOffset.UtcNow;
        currentSessionId = state.Question?.SessionId;
        answerDeadlineUtc = state.Question?.AnswerDeadlineUtc;
        eventTitleLabel.Text = state.EventName;
        questionEventLabel.Text = state.EventName;
        resultEventLabel.Text = state.EventName;
        leaderboardEventLabel.Text = state.EventName;
        UpdateParticipantCount(state.ParticipantCount);

        switch (DisplayPresentationResolver.Resolve(state))
        {
            case DisplayPresentationState.Joining:
                RenderWaiting(state);
                break;
            case DisplayPresentationState.QuestionClosed:
                RenderQuestion(state.Question, true);
                break;
            case DisplayPresentationState.QuestionOpen:
                RenderQuestion(state.Question, false);
                break;
            case DisplayPresentationState.AnswerRevealed:
                RenderResult(state.Question, state.Statistics);
                break;
            case DisplayPresentationState.Leaderboard:
                RenderLeaderboard(state.Leaderboard);
                break;
            default:
                ShowPanel(waitingPanel);
                waitingInstructionLabel.Text = "等待活動設定…";
                break;
        }
    }

    private void RenderWaiting(CurrentDisplayStateDto state)
    {
        countdownTimer.Stop();
        ShowPanel(waitingPanel);
        waitingInstructionLabel.Text = "掃描 QR Code 加入活動";
        joinCodeLabel.Text = state.JoinInfo is null ? "------" : state.JoinInfo.JoinCode;
        joinUrlLabel.Text = state.JoinInfo?.JoinUrl ?? string.Empty;
        joinUrlWarningLabel.Text = state.JoinInfo?.IsLoopback == true
            ? "⚠ localhost 僅供本機測試，手機無法連線"
            : string.Empty;
        if (state.JoinInfo is not null)
        {
            ReplaceQrImage(state.JoinInfo.JoinUrl);
        }
    }

    private void RenderQuestion(DisplayQuestionDto? question, bool isClosed)
    {
        if (question is null)
        {
            ShowPanel(waitingPanel);
            waitingInstructionLabel.Text = "等待題目…";
            return;
        }

        ShowPanel(questionPanel);
        var isPractice = question.Mode == QuizQuestionMode.Practice;
        questionEventLabel.Text = isPractice
            ? $"{questionEventLabel.Text.Split('｜')[0].Trim()}　｜　操作練習・本題不計分"
            : questionEventLabel.Text.Split('｜')[0].Trim();
        questionNumberLabel.Text = isPractice
            ? $"熱身 {question.QuestionNumber} / {question.TotalQuestionCount}"
            : $"第 {question.QuestionNumber} / {question.TotalQuestionCount} 題";
        questionTextLabel.Text = question.Text;
        questionTextLabel.Font = new Font(
            "Microsoft JhengHei UI",
            question.Text.Length switch { > 120 => 28F, > 80 => 34F, _ => 42F },
            FontStyle.Bold);
        var optionLabels = new[] { optionALabel, optionBLabel, optionCLabel, optionDLabel };
        for (var index = 0; index < optionLabels.Length; index++)
        {
            var option = question.Options.OrderBy(item => item.Order).ElementAtOrDefault(index);
            optionLabels[index].Text = option is null
                ? string.Empty
                : $"{(char)('A' + index)}. {option.Text}";
            optionLabels[index].Visible = option is not null;
            optionLabels[index].Font = new Font(
                "Microsoft JhengHei UI",
                option?.Text.Length > 70 ? 20F : 26F,
                FontStyle.Bold);
        }

        questionStatusLabel.Text = isClosed ? "時間到！等待公布答案" : "開放作答";
        questionStatusLabel.ForeColor = isClosed ? Color.FromArgb(255, 190, 82) : Color.FromArgb(94, 234, 170);
        questionProgressLabel.Text = $"已作答 {question.AnsweredCount:N0} / {question.ParticipantCount:N0}";
        answerDeadlineUtc = question.AnswerDeadlineUtc;
        if (isClosed)
        {
            countdownTimer.Stop();
            countdownLabel.Text = "00:00";
        }
        else
        {
            countdownTimer.Start();
            UpdateCountdown();
        }
    }

    private void RenderResult(
        DisplayQuestionDto? question,
        QuizSessionStatisticsDto? statistics)
    {
        countdownTimer.Stop();
        ShowPanel(resultPanel);
        if (question is null || statistics is null || !question.CorrectOptionId.HasValue)
        {
            correctAnswerLabel.Text = "等待結果資料…";
            correctRateLabel.Text = string.Empty;
            return;
        }

        var correctIndex = question.Options
            .OrderBy(option => option.Order)
            .ToList()
            .FindIndex(option => option.Id == question.CorrectOptionId.Value);
        var correctOption = question.Options.First(option => option.Id == question.CorrectOptionId.Value);
        correctAnswerLabel.Text = $"✓ 正確答案　{(char)('A' + correctIndex)}. {correctOption.Text}";
        var practiceMessage = question.Mode == QuizQuestionMode.Practice
            ? "操作練習・本題不計分　｜　"
            : string.Empty;
        var explanation = string.IsNullOrWhiteSpace(question.Explanation)
            ? string.Empty
            : $"{Environment.NewLine}說明：{question.Explanation}";
        correctRateLabel.Text = $"{practiceMessage}答對率 {statistics.CorrectRate:P1}{explanation}";
        correctRateLabel.Font = new Font(
            "Microsoft JhengHei UI",
            string.IsNullOrWhiteSpace(question.Explanation) ? 30F : 20F,
            FontStyle.Bold);
        var textLabels = new[] { resultOptionALabel, resultOptionBLabel, resultOptionCLabel, resultOptionDLabel };
        var bars = new[] { resultOptionABar, resultOptionBBar, resultOptionCBar, resultOptionDBar };
        var countLabels = new[] { resultOptionACountLabel, resultOptionBCountLabel, resultOptionCCountLabel, resultOptionDCountLabel };
        var distribution = statistics.OptionDistribution.OrderBy(item =>
            question.Options.First(option => option.Id == item.OptionId).Order).ToArray();
        var maximum = Math.Max(1, statistics.AnsweredCount);
        for (var index = 0; index < textLabels.Length; index++)
        {
            var item = distribution.ElementAtOrDefault(index);
            textLabels[index].Visible = bars[index].Visible = countLabels[index].Visible = item is not null;
            if (item is null)
            {
                continue;
            }

            var isCorrect = item.OptionId == question.CorrectOptionId;
            textLabels[index].Text = $"{(isCorrect ? "✓ " : string.Empty)}{(char)('A' + index)}. {item.Text}";
            textLabels[index].ForeColor = isCorrect ? Color.FromArgb(94, 234, 170) : Color.White;
            bars[index].Maximum = maximum;
            bars[index].Value = Math.Min(item.AnswerCount, maximum);
            countLabels[index].Text = item.AnswerCount.ToString("N0");
        }
    }

    private void RenderLeaderboard(IReadOnlyList<DisplayLeaderboardEntryDto> entries)
    {
        countdownTimer.Stop();
        ShowPanel(leaderboardPanel);
        leaderboardGrid.Rows.Clear();
        foreach (var entry in entries)
        {
            leaderboardGrid.Rows.Add(entry.Rank, Truncate(entry.DisplayName, 24), entry.TotalScore.ToString("N0"));
        }
    }

    private void UpdateQuestionProgress(QuestionProgressNotification progress)
    {
        if (currentSessionId != progress.SessionId)
        {
            return;
        }

        questionProgressLabel.Text = $"已作答 {progress.AnsweredCount:N0} / {progress.ParticipantCount:N0}";
    }

    private void UpdateParticipantCount(int participantCount)
    {
        participantCountLabel.Text = participantCount.ToString("N0");
    }

    private void countdownTimer_Tick(object? sender, EventArgs e) => UpdateCountdown();

    private void UpdateCountdown()
    {
        if (!answerDeadlineUtc.HasValue)
        {
            countdownLabel.Text = "--:--";
            return;
        }

        var remaining = answerDeadlineUtc.Value - (DateTimeOffset.UtcNow + serverClockOffset);
        var seconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));
        countdownLabel.Text = $"{seconds / 60:00}:{seconds % 60:00}";
        if (seconds == 0)
        {
            countdownTimer.Stop();
        }
    }

    private void ShowPanel(Control panel)
    {
        foreach (Control child in presentationPanel.Controls)
        {
            child.Visible = ReferenceEquals(child, panel);
        }

        panel.BringToFront();
    }

    private void ReplaceQrImage(string joinUrl)
    {
        var bytes = qrCodeGenerator.Generate(joinUrl);
        using var stream = new MemoryStream(bytes);
        using var source = Image.FromStream(stream);
        var replacement = new Bitmap(source);
        var previous = qrCodePictureBox.Image;
        qrCodePictureBox.Image = replacement;
        previous?.Dispose();
    }

    private void UpdateConnectionStatus(string message, bool isError)
    {
        connectionStatusLabel.Text = message;
        connectionStatusLabel.ForeColor = isError
            ? Color.FromArgb(255, 130, 130)
            : Color.FromArgb(94, 234, 170);
    }

    private void RunOnUiThread(Action action)
    {
        if (IsDisposed)
        {
            return;
        }

        if (InvokeRequired)
        {
            BeginInvoke(action);
            return;
        }

        action();
    }

    private void EnterFullscreen()
    {
        var screens = Screen.AllScreens;
        var index = screenComboBox.SelectedIndex;
        var screen = index >= 0 && index < screens.Length ? screens[index] : Screen.PrimaryScreen;
        if (screen is null)
        {
            return;
        }

        if (!isFullscreen)
        {
            previousBounds = Bounds;
            previousBorderStyle = FormBorderStyle;
        }

        isFullscreen = true;
        setupPanel.Visible = false;
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Normal;
        Bounds = screen.Bounds;
    }

    private void ExitFullscreen()
    {
        if (!isFullscreen)
        {
            return;
        }

        isFullscreen = false;
        FormBorderStyle = previousBorderStyle;
        Bounds = previousBounds;
        setupPanel.Visible = true;
    }

    private void DisplayForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            ExitFullscreen();
        }
        else if (e.KeyCode == Keys.F11)
        {
            if (isFullscreen)
            {
                ExitFullscreen();
            }
            else
            {
                EnterFullscreen();
            }
        }
    }

    private async void DisplayForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        countdownTimer.Stop();
        countdownTimer.Dispose();
        qrCodePictureBox.Image?.Dispose();
        await displayClient.DisposeAsync();
    }

    private static string Truncate(string value, int maximumLength) =>
        value.Length <= maximumLength ? value : value[..(maximumLength - 1)] + "…";
}
