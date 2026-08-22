namespace EventHub.Host;

internal partial class QuestionBankImportPreviewForm : Form
{
    public QuestionBankImportPreviewForm(QuestionBankPreviewView preview)
    {
        InitializeComponent();
        fileNameValueLabel.Text = preview.FileName;
        summaryLabel.Text =
            $"題庫：{preview.QuizTitle ?? "未識別"}　題數：{preview.QuestionCount}　錯誤：{preview.ErrorCount}　警告：{preview.WarningCount}";
        foreach (var row in preview.Rows.OrderBy(row => row.Order))
        {
            questionGrid.Rows.Add(
                row.Order,
                row.QuestionKey,
                row.Category,
                row.Difficulty,
                row.Question,
                ((char)('A' + row.CorrectOptionIndex)).ToString(),
                row.DurationSeconds);
        }

        foreach (var issue in preview.Issues)
        {
            issueGrid.Rows.Add(
                issue.Severity == QuestionBankIssueSeverity.Error ? "錯誤" : "警告",
                issue.RowNumber,
                issue.QuestionKey,
                issue.Field,
                issue.Message);
        }

        confirmButton.Enabled = preview.IsValid;
    }
}
