using EventHub.Domain.Quizzes;

namespace EventHub.Application.Quizzes;

public static class QuestionBankImportLimits
{
    public const long MaximumFileSizeBytes = 5 * 1024 * 1024;
    public const int MaximumQuestionCount = 500;
    public const int MinimumDurationSeconds = 5;
    public const int MaximumDurationSeconds = 120;

    public static readonly string[] RequiredHeaders =
    [
        "QuestionKey", "QuizTitle", "Category", "Difficulty", "Order", "Question",
        "OptionA", "OptionB", "OptionC", "OptionD", "CorrectOption", "DurationSeconds",
        "Explanation", "Mode"
    ];
}

public enum QuestionBankIssueSeverity
{
    Error,
    Warning
}

public sealed record QuestionBankIssue(
    int? RowNumber,
    string? QuestionKey,
    string Field,
    string Message,
    QuestionBankIssueSeverity Severity = QuestionBankIssueSeverity.Error);

public sealed record QuestionBankRawRow(
    int RowNumber,
    string QuestionKey,
    string QuizTitle,
    string Category,
    string Difficulty,
    string Order,
    string Question,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string CorrectOption,
    string DurationSeconds,
    string Explanation,
    string Mode);

public sealed record QuestionBankParseResult(
    IReadOnlyList<QuestionBankRawRow> Rows,
    IReadOnlyList<QuestionBankIssue> Issues);

public sealed record ValidatedQuestionBankRow(
    int RowNumber,
    string QuestionKey,
    string Category,
    QuizQuestionDifficulty Difficulty,
    QuizQuestionMode Mode,
    int Order,
    string Question,
    string? Explanation,
    IReadOnlyList<string> Options,
    int CorrectOptionIndex,
    int DurationSeconds);

public sealed record QuestionBankPreview(
    string FileName,
    string? QuizTitle,
    int QuestionCount,
    IReadOnlyList<ValidatedQuestionBankRow> Rows,
    IReadOnlyList<QuestionBankIssue> Issues)
{
    public bool IsValid => Issues.All(issue => issue.Severity != QuestionBankIssueSeverity.Error);

    public int ErrorCount => Issues.Count(issue => issue.Severity == QuestionBankIssueSeverity.Error);

    public int WarningCount => Issues.Count(issue => issue.Severity == QuestionBankIssueSeverity.Warning);
}

public sealed record QuestionBankSummary(Guid Id, string Title, int QuestionCount, DateTimeOffset CreatedAtUtc);

public enum QuestionBankQuestionStatus
{
    Pending,
    Current,
    Completed
}

public sealed record QuestionBankQuestionSummary(
    Guid Id,
    string QuestionKey,
    string? Category,
    QuizQuestionDifficulty Difficulty,
    QuizQuestionMode Mode,
    int Order,
    string Text,
    string? Explanation,
    IReadOnlyList<QuizOptionSummary> Options,
    int CorrectOptionIndex,
    int DurationSeconds,
    QuestionBankQuestionStatus Status);

public sealed record QuestionBankImportResult(
    Guid QuizId,
    string QuizTitle,
    int QuestionCount,
    DateTimeOffset ImportedAtUtc);

public interface IQuestionBankCsvParser
{
    Task<QuestionBankParseResult> ParseAsync(Stream stream, CancellationToken cancellationToken);
}
