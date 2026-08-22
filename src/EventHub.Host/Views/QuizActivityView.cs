namespace EventHub.Host.Views;

internal partial class QuizActivityView : UserControl
{
    private IReadOnlyList<QuestionBankQuestionView> questions = [];
    private bool isRendering;
    private DateTimeOffset? answerDeadlineUtc;

    public QuizActivityView()
    {
        InitializeComponent();
    }

    public event EventHandler? SelectedQuestionChanged;
    public event EventHandler? DefaultPracticeRequested;

    public QuestionBankQuestionView? SelectedQuestion =>
        questionListGrid.CurrentRow?.Tag as QuestionBankQuestionView;

    public bool HasScoredQuestions => questions.Any(question => question.Mode == QuizQuestionMode.Scored);

    public bool HasPracticeQuestions => questions.Any(question => question.Mode == QuizQuestionMode.Practice);

    public void RenderQuestions(IReadOnlyList<QuestionBankQuestionView> items, Guid? selectedQuestionId)
    {
        questions = items.OrderBy(question => question.Order).ToArray();
        isRendering = true;
        questionListGrid.Rows.Clear();
        foreach (var question in questions)
        {
            var modeQuestions = questions.Where(item => item.Mode == question.Mode).ToArray();
            var number = Array.FindIndex(modeQuestions, item => item.Id == question.Id) + 1;
            var prefix = question.Mode == QuizQuestionMode.Practice ? "P" : "Q";
            var marker = question.Status switch
            {
                QuestionBankQuestionStatus.Completed => "✓",
                QuestionBankQuestionStatus.Current => "●",
                _ => "○"
            };
            var rowIndex = questionListGrid.Rows.Add(
                $"{prefix}{number:00}",
                marker,
                question.Text);
            questionListGrid.Rows[rowIndex].Tag = question;
        }

        if (questionListGrid.Rows.Count > 0)
        {
            var selectedRow = questionListGrid.Rows.Cast<DataGridViewRow>()
                .FirstOrDefault(row => row.Tag is QuestionBankQuestionView question && question.Id == selectedQuestionId)
                ?? questionListGrid.Rows[0];
            questionListGrid.CurrentCell = selectedRow.Cells[0];
        }

        isRendering = false;
    }

    public void RenderQuestion(string quizTitle, QuizStateView state)
    {
        var selected = SelectedQuestion;
        var mode = state.Mode ?? selected?.Mode;
        quizTitleValueLabel.Text = quizTitle;
        modeValueLabel.Text = mode switch
        {
            QuizQuestionMode.Practice => "PRACTICE｜熱身題・不計分",
            QuizQuestionMode.Scored => "SCORED｜正式比賽",
            _ => "—"
        };
        modeValueLabel.ForeColor = mode == QuizQuestionMode.Practice ? Color.DarkOrange : Color.DarkGreen;
        questionNumberValueLabel.Text = GetPositionText(selected);
        stateValueLabel.Text = mode.HasValue ? state.State.ToString().ToUpperInvariant() : "NO QUIZ";
        progressValueLabel.Text = mode.HasValue
            ? $"已作答 {state.AnsweredCount} / {state.ParticipantCount}　在線 {state.OnlineCount}"
            : "尚未選擇題庫";
        questionTextLabel.Text = state.QuestionText ?? selected?.Text ?? "尚未選擇題庫";
        questionTextLabel.Font = new Font(
            "Microsoft JhengHei UI",
            questionTextLabel.Text.Length switch { > 180 => 11F, > 100 => 13F, _ => 16F },
            FontStyle.Bold);
        var options = state.Options.Count > 0 ? state.Options : selected?.Options ?? [];
        RenderOption(optionALabel, options, 0, "A");
        RenderOption(optionBLabel, options, 1, "B");
        RenderOption(optionCLabel, options, 2, "C");
        RenderOption(optionDLabel, options, 3, "D");
        answerDeadlineUtc = state.State == QuizState.Open ? state.AnswerDeadlineUtc : null;
        practiceCompletePanel.Visible = false;
        questionTabPage.Text = "題目";
        UpdateCountdown();
    }

    public void SetDefaultPracticeAvailability(bool visible, bool enabled)
    {
        defaultPracticeButton.Visible = visible;
        defaultPracticeButton.Enabled = enabled;
    }

    public void RenderResult(
        QuizStatisticsView statistics,
        QuizLeaderboardView leaderboard,
        Guid? correctOptionId,
        string? explanation)
    {
        correctAnswerValueLabel.Text = correctOptionId.HasValue
            ? statistics.OptionDistribution
                .Select((option, index) => new { option.OptionId, Text = $"{(char)('A' + index)}. {option.Text}" })
                .FirstOrDefault(option => option.OptionId == correctOptionId.Value)?.Text ?? "已公布"
            : "等待公布";
        resultSummaryValueLabel.Text =
            $"作答 {statistics.AnsweredCount}　答對 {statistics.CorrectCount}　答錯 {statistics.IncorrectCount}　" +
            $"未作答 {statistics.NoAnswerCount}　正確率 {statistics.CorrectRate:P1}";
        explanationTextBox.Text = string.IsNullOrWhiteSpace(explanation) ? "本題沒有答案說明。" : explanation;
        distributionGrid.Rows.Clear();
        foreach (var (option, index) in statistics.OptionDistribution.Select((option, index) => (option, index)))
        {
            distributionGrid.Rows.Add($"{(char)('A' + index)}. {option.Text}", option.AnswerCount);
        }

        leaderboardGrid.Rows.Clear();
        foreach (var entry in leaderboard.Entries)
        {
            leaderboardGrid.Rows.Add(entry.Rank, entry.DisplayName, entry.TotalScore, entry.CorrectCount, entry.AnsweredCount);
        }

        activityTabControl.SelectedTab = resultTabPage;
    }

    public void ClearResult()
    {
        correctAnswerValueLabel.Text = "等待公布";
        resultSummaryValueLabel.Text = "尚無結果";
        explanationTextBox.Text = string.Empty;
        distributionGrid.Rows.Clear();
        leaderboardGrid.Rows.Clear();
    }

    public void ShowPracticeCompleted(int participantCount, int answeredCount)
    {
        practiceCompletePanel.Visible = true;
        practiceCompletePanel.BringToFront();
        practiceParticipantValueLabel.Text = participantCount.ToString();
        practiceAnsweredValueLabel.Text = answeredCount.ToString();
        practiceNotAnsweredValueLabel.Text = Math.Max(0, participantCount - answeredCount).ToString();
        activityTabControl.SelectedTab = questionTabPage;
    }

    public void ShowQuestion() => activityTabControl.SelectedTab = questionTabPage;

    public void ShowFinalLeaderboard() => activityTabControl.SelectedTab = leaderboardTabPage;

    public bool SelectFirst(QuizQuestionMode mode)
    {
        var row = questionListGrid.Rows.Cast<DataGridViewRow>()
            .FirstOrDefault(item => item.Tag is QuestionBankQuestionView question && question.Mode == mode);
        return SelectRow(row);
    }

    public bool MoveToNextInMode(QuizQuestionMode mode)
    {
        var currentOrder = SelectedQuestion?.Order ?? 0;
        var row = questionListGrid.Rows.Cast<DataGridViewRow>()
            .FirstOrDefault(item => item.Tag is QuestionBankQuestionView question &&
                question.Mode == mode && question.Order > currentOrder);
        return SelectRow(row);
    }

    public bool HasNextInMode(QuizQuestionMode mode)
    {
        var currentOrder = SelectedQuestion?.Order ?? 0;
        return questions.Any(question => question.Mode == mode && question.Order > currentOrder);
    }

    public void SetNavigationEnabled(bool enabled) => questionListGrid.Enabled = enabled;

    public void UpdateProgress(QuestionProgressNotification progress)
    {
        progressValueLabel.Text =
            $"已作答 {progress.AnsweredCount} / {progress.ParticipantCount}　在線 {progress.OnlineCount}";
    }

    private bool SelectRow(DataGridViewRow? row)
    {
        if (row is null)
        {
            return false;
        }

        questionListGrid.CurrentCell = row.Cells[0];
        return true;
    }

    private string GetPositionText(QuestionBankQuestionView? selected)
    {
        if (selected is null)
        {
            return "— / —";
        }

        var modeQuestions = questions.Where(question => question.Mode == selected.Mode).ToArray();
        var number = Array.FindIndex(modeQuestions, question => question.Id == selected.Id) + 1;
        return selected.Mode == QuizQuestionMode.Practice
            ? $"Practice {number} / {modeQuestions.Length}"
            : $"Question {number} / {modeQuestions.Length}";
    }

    private static void RenderOption(Label label, IReadOnlyList<QuestionOptionView> options, int index, string prefix)
    {
        var text = index < options.Count ? options[index].Text : "—";
        label.Text = $"{prefix}. {text}";
        label.Font = new Font("Microsoft JhengHei UI", text.Length > 80 ? 9F : 11F, FontStyle.Bold);
    }

    private void questionListGrid_SelectionChanged(object? sender, EventArgs e)
    {
        if (!isRendering)
        {
            SelectedQuestionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void defaultPracticeButton_Click(object? sender, EventArgs e) =>
        DefaultPracticeRequested?.Invoke(this, EventArgs.Empty);

    private void countdownTimer_Tick(object? sender, EventArgs e) => UpdateCountdown();

    private void UpdateCountdown()
    {
        if (!answerDeadlineUtc.HasValue)
        {
            countdownValueLabel.Text = "--:--";
            return;
        }

        var remaining = answerDeadlineUtc.Value - DateTimeOffset.UtcNow;
        var seconds = Math.Max(0, (int)Math.Ceiling(remaining.TotalSeconds));
        countdownValueLabel.Text = $"{seconds / 60:00}:{seconds % 60:00}";
    }
}
