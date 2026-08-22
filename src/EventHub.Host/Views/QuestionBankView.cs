namespace EventHub.Host.Views;

internal partial class QuestionBankView : UserControl
{
    private bool isRendering;

    public QuestionBankView()
    {
        InitializeComponent();
    }

    public event EventHandler? ImportRequested;
    public event EventHandler? ExportTemplateRequested;
    public event EventHandler? RefreshRequested;
    public event EventHandler? SelectedBankChanged;
    public event EventHandler? SelectedQuestionChanged;

    public QuestionBankSummaryView? SelectedBank => questionBankComboBox.SelectedItem as QuestionBankSummaryView;
    public QuestionBankQuestionView? SelectedQuestion => questionGrid.CurrentRow?.Tag as QuestionBankQuestionView;
    public bool HasNextQuestion => questionGrid.CurrentRow is not null && questionGrid.CurrentRow.Index < questionGrid.Rows.Count - 1;

    public void RenderBanks(IReadOnlyCollection<QuestionBankSummaryView> banks, Guid? selectedQuizId = null)
    {
        isRendering = true;
        questionBankComboBox.BeginUpdate();
        questionBankComboBox.Items.Clear();
        questionBankComboBox.Items.AddRange(banks.Cast<object>().ToArray());
        questionBankComboBox.EndUpdate();
        if (banks.Count > 0)
        {
            var index = selectedQuizId.HasValue
                ? banks.ToList().FindIndex(bank => bank.Id == selectedQuizId.Value)
                : 0;
            questionBankComboBox.SelectedIndex = index >= 0 ? index : 0;
        }
        else
        {
            questionGrid.Rows.Clear();
            bankSummaryLabel.Text = "目前沒有題庫。";
        }

        isRendering = false;
    }

    public void RenderQuestions(IReadOnlyList<QuestionBankQuestionView> items, Guid? selectedQuestionId)
    {
        isRendering = true;
        questionGrid.Rows.Clear();
        foreach (var question in items.OrderBy(question => question.Order))
        {
            var index = questionGrid.Rows.Add(
                question.Order,
                question.QuestionKey,
                question.Category,
                question.Difficulty,
                question.Mode,
                question.Text,
                question.DurationSeconds,
                question.Status);
            questionGrid.Rows[index].Tag = question;
        }

        if (questionGrid.Rows.Count > 0)
        {
            var selectedRow = questionGrid.Rows.Cast<DataGridViewRow>()
                .FirstOrDefault(row => row.Tag is QuestionBankQuestionView question && question.Id == selectedQuestionId)
                ?? questionGrid.Rows[0];
            questionGrid.CurrentCell = selectedRow.Cells[0];
        }
        else
        {
            questionDetailTextBox.Text = "此題庫目前沒有題目。";
        }

        var bank = SelectedBank;
        bankSummaryLabel.Text = bank is null
            ? "尚未選擇題庫"
            : $"{bank.Title}　{bank.QuestionCount} 題　建立時間：{bank.CreatedAtUtc.ToLocalTime():yyyy/MM/dd HH:mm}";
        isRendering = false;
        RenderSelectedQuestion();
    }

    public bool MoveSelection(int offset)
    {
        if (questionGrid.Rows.Count == 0)
        {
            return false;
        }

        var index = Math.Clamp((questionGrid.CurrentRow?.Index ?? 0) + offset, 0, questionGrid.Rows.Count - 1);
        questionGrid.CurrentCell = questionGrid.Rows[index].Cells[0];
        return true;
    }

    public void SetNavigationEnabled(bool enabled)
    {
        previousButton.Enabled = enabled && (questionGrid.CurrentRow?.Index ?? 0) > 0;
        nextButton.Enabled = enabled && HasNextQuestion;
        questionGrid.Enabled = enabled;
    }

    public void SetBusy(bool isBusy)
    {
        importButton.Enabled = !isBusy;
        exportTemplateButton.Enabled = !isBusy;
        refreshButton.Enabled = !isBusy;
    }

    public void SetStatus(string message, bool isError = false)
    {
        statusLabel.Text = message;
        statusLabel.ForeColor = isError ? Color.Firebrick : Color.DarkGreen;
    }

    private void questionBankComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!isRendering)
        {
            SelectedBankChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void questionGrid_SelectionChanged(object? sender, EventArgs e)
    {
        if (!isRendering)
        {
            RenderSelectedQuestion();
        }
    }

    private void RenderSelectedQuestion()
    {
        var question = SelectedQuestion;
        if (question is null)
        {
            return;
        }

        var options = question.Options.OrderBy(option => option.Order)
            .Select(option => $"{(char)('A' + option.Order)}. {option.Text}");
        questionDetailTextBox.Text =
            $"{question.Text}{Environment.NewLine}{string.Join("　", options)}{Environment.NewLine}" +
            $"模式：{question.Mode}　正確答案：{(char)('A' + question.CorrectOptionIndex)}　秒數：{question.DurationSeconds}　狀態：{question.Status}" +
            $"{Environment.NewLine}答案說明：{question.Explanation ?? "（無）"}";
        SelectedQuestionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void importButton_Click(object? sender, EventArgs e) => ImportRequested?.Invoke(this, EventArgs.Empty);
    private void exportTemplateButton_Click(object? sender, EventArgs e) => ExportTemplateRequested?.Invoke(this, EventArgs.Empty);
    private void refreshButton_Click(object? sender, EventArgs e) => RefreshRequested?.Invoke(this, EventArgs.Empty);
    private void previousButton_Click(object? sender, EventArgs e) => MoveSelection(-1);
    private void nextButton_Click(object? sender, EventArgs e) => MoveSelection(1);
}
