using EventHub.Domain.Quizzes;

namespace EventHub.Application.Quizzes;

public sealed class QuestionBankValidator
{
    public (string? QuizTitle, IReadOnlyList<ValidatedQuestionBankRow> Rows, IReadOnlyList<QuestionBankIssue> Issues)
        Validate(QuestionBankParseResult parseResult)
    {
        var issues = parseResult.Issues.ToList();
        var rows = new List<ValidatedQuestionBankRow>();
        if (parseResult.Rows.Count == 0)
        {
            issues.Add(new QuestionBankIssue(null, null, "File", "CSV 不可為空白，且至少需要一筆題目。"));
            return (null, rows, issues);
        }

        if (parseResult.Rows.Count > QuestionBankImportLimits.MaximumQuestionCount)
        {
            issues.Add(new QuestionBankIssue(
                null,
                null,
                "File",
                $"題目數不可超過 {QuestionBankImportLimits.MaximumQuestionCount} 題。"));
        }

        var titles = parseResult.Rows.Select(row => row.QuizTitle.Trim())
            .Where(title => !string.IsNullOrWhiteSpace(title))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var quizTitle = titles.Length == 1 ? titles[0] : null;
        if (titles.Length != 1 || parseResult.Rows.Any(row => string.IsNullOrWhiteSpace(row.QuizTitle)))
        {
            issues.Add(new QuestionBankIssue(null, null, "QuizTitle", "一個 CSV 必須且只能包含一個非空白 QuizTitle。"));
        }
        else if (quizTitle!.Length > 200)
        {
            issues.Add(new QuestionBankIssue(null, null, "QuizTitle", "QuizTitle 不可超過 200 個字元。"));
        }

        foreach (var row in parseResult.Rows)
        {
            ValidateRow(row, rows, issues);
        }

        AddDuplicateIssues(parseResult.Rows, issues, row => row.QuestionKey.Trim(), "QuestionKey", "QuestionKey 不可重複。");
        AddDuplicateIssues(
            parseResult.Rows,
            issues,
            row => int.TryParse(row.Order.Trim(), out var parsedOrder)
                ? parsedOrder.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : row.Order.Trim(),
            "Order",
            "Order 不可重複。");
        return (quizTitle, rows, issues);
    }

    private static void ValidateRow(
        QuestionBankRawRow row,
        ICollection<ValidatedQuestionBankRow> validRows,
        ICollection<QuestionBankIssue> issues)
    {
        var before = issues.Count;
        var key = Required(row, row.QuestionKey, "QuestionKey", 50, issues);
        var category = row.Category.Trim();
        if (category.Length > 100)
        {
            Add(row, "Category", "Category 不可超過 100 個字元。", issues);
        }

        var difficultyText = row.Difficulty.Trim();
        var difficulty = QuizQuestionDifficulty.Medium;
        if (!Enum.GetNames<QuizQuestionDifficulty>()
                .Contains(difficultyText, StringComparer.OrdinalIgnoreCase) ||
            !Enum.TryParse(difficultyText, true, out difficulty))
        {
            Add(row, "Difficulty", "Difficulty 必須為 Easy、Medium 或 Hard。", issues);
        }

        var modeText = row.Mode.Trim();
        var mode = QuizQuestionMode.Scored;
        if (!Enum.GetNames<QuizQuestionMode>().Contains(modeText, StringComparer.OrdinalIgnoreCase) ||
            !Enum.TryParse(modeText, true, out mode))
        {
            Add(row, "Mode", "Mode 必須為 Practice 或 Scored。", issues);
        }

        if (!int.TryParse(row.Order.Trim(), out var order) || order < 1)
        {
            Add(row, "Order", "Order 必須為大於等於 1 的整數。", issues);
        }

        var question = Required(row, row.Question, "Question", 500, issues);
        var explanation = row.Explanation.Trim();
        if (explanation.Length > 1000)
        {
            Add(row, "Explanation", "Explanation 不可超過 1000 個字元。", issues);
        }
        var options = new[]
        {
            Required(row, row.OptionA, "OptionA", 200, issues),
            Required(row, row.OptionB, "OptionB", 200, issues),
            Required(row, row.OptionC, "OptionC", 200, issues),
            Required(row, row.OptionD, "OptionD", 200, issues)
        };
        if (options.All(option => !string.IsNullOrWhiteSpace(option)) &&
            options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Length)
        {
            Add(row, "Options", "A、B、C、D 選項內容不可重複。", issues);
        }

        var correctText = row.CorrectOption.Trim().ToUpperInvariant();
        var correctIndex = correctText.Length == 1 ? correctText[0] - 'A' : -1;
        if (correctIndex is < 0 or > 3)
        {
            Add(row, "CorrectOption", "CorrectOption 必須為 A、B、C 或 D。", issues);
        }

        if (!int.TryParse(row.DurationSeconds.Trim(), out var duration) ||
            duration < QuestionBankImportLimits.MinimumDurationSeconds ||
            duration > QuestionBankImportLimits.MaximumDurationSeconds)
        {
            Add(
                row,
                "DurationSeconds",
                $"DurationSeconds 必須介於 {QuestionBankImportLimits.MinimumDurationSeconds} 到 {QuestionBankImportLimits.MaximumDurationSeconds}。",
                issues);
        }

        if (issues.Count == before)
        {
            validRows.Add(new ValidatedQuestionBankRow(
                row.RowNumber,
                key,
                category,
                difficulty,
                mode,
                order,
                question,
                string.IsNullOrWhiteSpace(explanation) ? null : explanation,
                options,
                correctIndex,
                duration));
        }
    }

    private static string Required(
        QuestionBankRawRow row,
        string value,
        string field,
        int maximumLength,
        ICollection<QuestionBankIssue> issues)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            Add(row, field, $"{field} 不可空白。", issues);
        }
        else if (normalized.Length > maximumLength)
        {
            Add(row, field, $"{field} 不可超過 {maximumLength} 個字元。", issues);
        }

        return normalized;
    }

    private static void AddDuplicateIssues(
        IEnumerable<QuestionBankRawRow> rows,
        ICollection<QuestionBankIssue> issues,
        Func<QuestionBankRawRow, string> selector,
        string field,
        string message)
    {
        foreach (var group in rows.Where(row => !string.IsNullOrWhiteSpace(selector(row)))
                     .GroupBy(selector, StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1))
        {
            foreach (var row in group)
            {
                Add(row, field, message, issues);
            }
        }
    }

    private static void Add(
        QuestionBankRawRow row,
        string field,
        string message,
        ICollection<QuestionBankIssue> issues) =>
        issues.Add(new QuestionBankIssue(row.RowNumber, row.QuestionKey.Trim(), field, message));
}
