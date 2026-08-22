namespace EventHub.Host;

internal sealed record QuestionBankIssueView(
    int? RowNumber,
    string? QuestionKey,
    string Field,
    string Message,
    QuestionBankIssueSeverity Severity);

internal enum QuestionBankIssueSeverity
{
    Error,
    Warning
}

internal sealed record QuestionBankPreviewView(
    string FileName,
    string? QuizTitle,
    int QuestionCount,
    IReadOnlyList<QuestionBankRowView> Rows,
    IReadOnlyList<QuestionBankIssueView> Issues,
    bool IsValid,
    int ErrorCount,
    int WarningCount);

internal sealed record QuestionBankRowView(
    int RowNumber,
    string QuestionKey,
    string Category,
    QuestionBankDifficulty Difficulty,
    int Order,
    string Question,
    IReadOnlyList<string> Options,
    int CorrectOptionIndex,
    int DurationSeconds);

internal sealed record QuestionBankSummaryView(Guid Id, string Title, int QuestionCount, DateTimeOffset CreatedAtUtc)
{
    public override string ToString() => $"{Title}（{QuestionCount} 題）";
}

internal sealed record QuestionBankQuestionView(
    Guid Id,
    string QuestionKey,
    string? Category,
    QuestionBankDifficulty Difficulty,
    int Order,
    string Text,
    IReadOnlyList<QuestionOptionView> Options,
    int CorrectOptionIndex,
    int DurationSeconds,
    QuestionBankQuestionStatus Status);

internal sealed record QuestionOptionView(Guid Id, string Text, int Order);

internal sealed record QuestionBankImportResultView(Guid QuizId, string QuizTitle, int QuestionCount);

internal enum QuestionBankDifficulty
{
    Easy,
    Medium,
    Hard
}

internal enum QuestionBankQuestionStatus
{
    Pending,
    Current,
    Completed
}
