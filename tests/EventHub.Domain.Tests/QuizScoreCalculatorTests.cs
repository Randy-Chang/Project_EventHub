using EventHub.Domain.Quizzes;

namespace EventHub.Domain.Tests;

public sealed class QuizScoreCalculatorTests
{
    private static readonly DateTimeOffset Start = new(2026, 8, 22, 4, 0, 0, TimeSpan.Zero);
    private readonly QuizScoreCalculator calculator = new();

    [Fact]
    public void CorrectImmediately_ReturnsMaximumScore()
    {
        var result = calculator.Calculate(true, Start, Start.AddSeconds(20), Start);

        Assert.Equal(500, result.BaseScore);
        Assert.Equal(500, result.SpeedBonus);
        Assert.Equal(1000, result.Score);
    }

    [Fact]
    public void CorrectAtHalfTime_Returns750()
    {
        var result = calculator.Calculate(true, Start, Start.AddSeconds(20), Start.AddSeconds(10));

        Assert.Equal(250, result.SpeedBonus);
        Assert.Equal(750, result.Score);
    }

    [Fact]
    public void CorrectAtDeadline_ReturnsBaseScore()
    {
        var result = calculator.Calculate(true, Start, Start.AddSeconds(20), Start.AddSeconds(20));

        Assert.Equal(0, result.SpeedBonus);
        Assert.Equal(500, result.Score);
    }

    [Fact]
    public void IncorrectAnswer_ReturnsZero()
    {
        var result = calculator.Calculate(false, Start, Start.AddSeconds(20), Start.AddSeconds(1));

        Assert.Equal(0, result.BaseScore);
        Assert.Equal(0, result.SpeedBonus);
        Assert.Equal(0, result.Score);
    }

    [Fact]
    public void SubmissionBeforeStart_IsClampedToStart()
    {
        var result = calculator.Calculate(true, Start, Start.AddSeconds(20), Start.AddSeconds(-2));

        Assert.Equal(1000, result.Score);
        Assert.Equal(0, result.ElapsedTicks);
    }

    [Fact]
    public void SubmissionAfterDeadline_IsClampedToDeadline()
    {
        var result = calculator.Calculate(true, Start, Start.AddSeconds(20), Start.AddSeconds(30));

        Assert.Equal(500, result.Score);
        Assert.Equal(TimeSpan.FromSeconds(20).Ticks, result.ElapsedTicks);
    }
}
