using EventHub.Application.Quizzes;

namespace EventHub.Application.Tests;

public sealed class QuestionBankValidatorTests
{
    private readonly QuestionBankValidator validator = new();

    [Fact]
    public void ValidRows_AreAccepted()
    {
        var result = Validate(Row());
        Assert.Empty(result.Issues);
        Assert.Single(result.Rows);
    }

    [Fact]
    public void EmptyCsv_IsRejected() => AssertIssue(new QuestionBankParseResult([], []), "File");

    [Fact]
    public void MultipleQuizTitles_AreRejected() =>
        AssertIssue(Parse(Row(), Row() with { RowNumber = 3, QuestionKey = "Q2", Order = "2", QuizTitle = "另一題庫" }), "QuizTitle");

    [Fact]
    public void DuplicateQuestionKey_IsRejected() =>
        AssertIssue(Parse(Row(), Row() with { RowNumber = 3, Order = "2" }), "QuestionKey");

    [Fact]
    public void DuplicateOrder_IsRejected() =>
        AssertIssue(Parse(Row(), Row() with { RowNumber = 3, QuestionKey = "Q2" }), "Order");

    [Theory]
    [InlineData("Beginner")]
    [InlineData("1")]
    [InlineData("")]
    public void InvalidDifficulty_IsRejected(string value) =>
        AssertIssue(Parse(Row() with { Difficulty = value }), "Difficulty");

    [Theory]
    [InlineData("E")]
    [InlineData("")]
    public void InvalidCorrectOption_IsRejected(string value) =>
        AssertIssue(Parse(Row() with { CorrectOption = value }), "CorrectOption");

    [Theory]
    [InlineData("4")]
    [InlineData("121")]
    [InlineData("abc")]
    public void InvalidDuration_IsRejected(string value) =>
        AssertIssue(Parse(Row() with { DurationSeconds = value }), "DurationSeconds");

    [Fact]
    public void MissingRequiredText_IsRejected() =>
        AssertIssue(Parse(Row() with { Question = " " }), "Question");

    [Fact]
    public void DuplicateOptions_AreRejected() =>
        AssertIssue(Parse(Row() with { OptionD = "A" }), "Options");

    [Fact]
    public void LowerCaseDifficulty_IsAccepted() =>
        Assert.Empty(Validate(Row() with { Difficulty = "hard" }).Issues);

    private (string? QuizTitle, IReadOnlyList<ValidatedQuestionBankRow> Rows, IReadOnlyList<QuestionBankIssue> Issues)
        Validate(QuestionBankRawRow row) => validator.Validate(Parse(row));

    private void AssertIssue(QuestionBankParseResult result, string field) =>
        Assert.Contains(validator.Validate(result).Issues, issue => issue.Field == field);

    private static QuestionBankParseResult Parse(params QuestionBankRawRow[] rows) => new(rows, []);

    private static QuestionBankRawRow Row() =>
        new(2, "Q1", "尾牙題庫", "公司", "Medium", "1", "題目", "A", "B", "C", "D", "B", "20");
}
