using EventHub.Domain.Quizzes;

namespace EventHub.Domain.Tests;

public sealed class QuizQuestionMetadataTests
{
    [Fact]
    public void Create_WithMetadata_NormalizesAndMapsCorrectOption()
    {
        var question = QuizQuestion.Create(
            Guid.NewGuid(),
            " Q-001 ",
            " 公司歷史 ",
            QuizQuestionDifficulty.Hard,
            "題目",
            ["A", "B", "C", "D"],
            2,
            TimeSpan.FromSeconds(20),
            1);

        Assert.Equal("Q-001", question.QuestionKey);
        Assert.Equal("公司歷史", question.Category);
        Assert.Equal(QuizQuestionDifficulty.Hard, question.Difficulty);
        Assert.Equal(question.Options.OrderBy(option => option.Order).ElementAt(2).Id, question.CorrectOptionId);
    }
}
