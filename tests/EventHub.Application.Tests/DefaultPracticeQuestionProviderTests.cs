using System.Text;
using EventHub.Application.Quizzes;

namespace EventHub.Application.Tests;

public sealed class DefaultPracticeQuestionProviderTests
{
    [Fact]
    public void GetQuestions_ReturnsTwoValidPracticeQuestions()
    {
        var questions = new DefaultPracticeQuestionProvider().GetQuestions();

        Assert.Equal(2, questions.Count);
        Assert.Equal(questions.Count, questions.Select(item => item.QuestionKey).Distinct().Count());
        Assert.All(questions, item =>
        {
            Assert.StartsWith("DEFAULT-P", item.QuestionKey);
            Assert.Equal(EventHub.Domain.Quizzes.QuizQuestionMode.Practice, item.Mode);
            Assert.Equal(10, item.DurationSeconds);
            Assert.Equal(4, item.Options.Count);
            Assert.InRange(item.CorrectOptionIndex, 0, item.Options.Count - 1);
            Assert.False(string.IsNullOrWhiteSpace(item.Explanation));
        });
        Assert.Equal("2", questions[0].Options[questions[0].CorrectOptionIndex]);
        Assert.Equal("尾牙", questions[1].Options[questions[1].CorrectOptionIndex]);
    }

    [Fact]
    public void CsvTemplate_UsesUtf8BomCrLfAndContainsPracticeAndScoredRows()
    {
        var bytes = QuestionBankCsvTemplate.CreateUtf8BomBytes();
        var preamble = Encoding.UTF8.GetPreamble();
        Assert.True(bytes.Take(preamble.Length).SequenceEqual(preamble));

        var content = Encoding.UTF8.GetString(bytes[preamble.Length..]);
        Assert.Contains("Explanation,Mode\r\n", content);
        Assert.Contains(",Practice\r\n", content);
        Assert.Contains(",Scored\r\n", content);
        Assert.Contains("\"哪個選項包含逗號,但仍應正確解析？\"", content);
        Assert.DoesNotContain("\n", content.Replace("\r\n", string.Empty));
    }
}
