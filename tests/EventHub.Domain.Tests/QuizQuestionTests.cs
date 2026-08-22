using EventHub.Domain.Common;
using EventHub.Domain.Quizzes;

namespace EventHub.Domain.Tests;

public sealed class QuizQuestionTests
{
    [Fact]
    public void Create_BuildsOptionsAndSelectsCorrectAnswer()
    {
        var question = QuizQuestion.Create(
            Guid.NewGuid(),
            "  公司成立於哪一年？ ",
            ["2000", "2005", "2010"],
            1,
            TimeSpan.FromSeconds(15),
            1);

        Assert.Equal("公司成立於哪一年？", question.Text);
        Assert.Equal(3, question.Options.Count);
        Assert.Equal(question.Options.ElementAt(1).Id, question.CorrectOptionId);
        Assert.All(question.Options, option => Assert.Equal(question.Id, option.QuizQuestionId));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void Create_RejectsUnsupportedOptionCount(int optionCount)
    {
        var options = Enumerable.Range(1, optionCount).Select(index => $"選項 {index}").ToArray();

        Assert.Throws<DomainValidationException>(() => QuizQuestion.Create(
            Guid.NewGuid(),
            "題目",
            options,
            0,
            TimeSpan.FromSeconds(15),
            1));
    }
}
