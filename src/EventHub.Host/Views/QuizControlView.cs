namespace EventHub.Host.Views;

internal partial class QuizControlView : UserControl
{
    private QuizPrimaryAction currentAction;
    private bool isPrimaryActionAvailable;
    private DateTimeOffset? answerDeadlineUtc;

    public QuizControlView()
    {
        InitializeComponent();
    }

    public event EventHandler? PrimaryActionRequested;
    public event EventHandler? ManualQuestionCreateRequested;

    public QuizPrimaryAction CurrentAction => currentAction;

    public CreateQuestionRequest BuildManualQuestionRequest()
    {
        var options = new[]
        {
            manualOptionATextBox.Text,
            manualOptionBTextBox.Text,
            manualOptionCTextBox.Text,
            manualOptionDTextBox.Text
        }.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
        if (manualCorrectOptionComboBox.SelectedIndex < 0 ||
            manualCorrectOptionComboBox.SelectedIndex >= options.Length)
        {
            throw new InvalidOperationException("請選擇存在的正確答案選項。");
        }

        return new CreateQuestionRequest(
            manualQuestionTextBox.Text,
            options,
            manualCorrectOptionComboBox.SelectedIndex,
            (int)manualDurationNumeric.Value);
    }

    public void Render(
        string quizTitle,
        string questionPosition,
        QuestionBankQuestionView? selectedQuestion,
        QuizStateView state,
        bool hasNextQuestion)
    {
        quizTitleValueLabel.Text = quizTitle;
        questionPositionLabel.Text = questionPosition;
        questionStateValueLabel.Text = state.State.ToString();
        progressValueLabel.Text = $"已作答 {state.AnsweredCount} / {state.ParticipantCount}　在線 {state.OnlineCount}";
        answerDeadlineUtc = state.State == QuizState.Open ? state.AnswerDeadlineUtc : null;
        var text = state.QuestionText ?? selectedQuestion?.Text ?? "請先到題庫選擇題目";
        questionTextLabel.Text = text;
        var options = state.Options.Count > 0 ? state.Options : selectedQuestion?.Options ?? [];
        RenderOption(optionALabel, options, 0, "A");
        RenderOption(optionBLabel, options, 1, "B");
        RenderOption(optionCLabel, options, 2, "C");
        RenderOption(optionDLabel, options, 3, "D");
        var presentation = QuizActionPresentation.Resolve(
            state.State,
            selectedQuestion is not null || state.QuestionId.HasValue,
            hasNextQuestion);
        currentAction = presentation.Action;
        isPrimaryActionAvailable = presentation.IsEnabled;
        primaryActionButton.Text = presentation.ButtonText;
        primaryActionButton.Enabled = presentation.IsEnabled;
        guidanceLabel.Text = presentation.Guidance;
        UpdateCountdown();
    }

    public void UpdateProgress(QuestionProgressNotification progress)
    {
        progressValueLabel.Text =
            $"已作答 {progress.AnsweredCount} / {progress.ParticipantCount}　在線 {progress.OnlineCount}";
    }

    public void SetBusy(bool isBusy)
    {
        primaryActionButton.Enabled = !isBusy && isPrimaryActionAvailable;
        createManualQuestionButton.Enabled = !isBusy;
    }

    public void SetStatus(string message, bool isError = false)
    {
        statusLabel.Text = message;
        statusLabel.ForeColor = isError ? Color.Firebrick : Color.DarkGreen;
    }

    private static void RenderOption(Label label, IReadOnlyList<QuestionOptionView> options, int index, string prefix)
    {
        label.Text = index < options.Count ? $"{prefix}. {options[index].Text}" : $"{prefix}. —";
    }

    private void UpdateCountdown()
    {
        if (!answerDeadlineUtc.HasValue)
        {
            countdownValueLabel.Text = "--:--";
            return;
        }

        var remaining = answerDeadlineUtc.Value - DateTimeOffset.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            countdownValueLabel.Text = "00:00";
            return;
        }

        countdownValueLabel.Text = $"{(int)remaining.TotalMinutes:00}:{remaining.Seconds:00}";
    }

    private void countdownTimer_Tick(object? sender, EventArgs e) => UpdateCountdown();
    private void primaryActionButton_Click(object? sender, EventArgs e) => PrimaryActionRequested?.Invoke(this, EventArgs.Empty);
    private void createManualQuestionButton_Click(object? sender, EventArgs e) =>
        ManualQuestionCreateRequested?.Invoke(this, EventArgs.Empty);
}
