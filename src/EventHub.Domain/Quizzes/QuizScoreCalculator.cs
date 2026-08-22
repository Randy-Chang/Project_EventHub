namespace EventHub.Domain.Quizzes;

public sealed class QuizScoreCalculator
{
    public const int CorrectAnswerBaseScore = 500;
    public const int MaximumSpeedBonus = 500;

    public QuizScoreCalculation Calculate(
        bool isCorrect,
        DateTimeOffset startedAtUtc,
        DateTimeOffset answerDeadlineUtc,
        DateTimeOffset submittedAtUtc)
    {
        var duration = answerDeadlineUtc - startedAtUtc;
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(answerDeadlineUtc), "作答截止時間必須晚於開始時間。");
        }

        var elapsed = submittedAtUtc - startedAtUtc;
        var clampedElapsedTicks = Math.Clamp(elapsed.Ticks, 0, duration.Ticks);
        if (!isCorrect)
        {
            return new QuizScoreCalculation(false, 0, 0, 0, clampedElapsedTicks);
        }

        var remainingRatio = 1m - ((decimal)clampedElapsedTicks / duration.Ticks);
        var speedBonus = (int)Math.Round(
            MaximumSpeedBonus * remainingRatio,
            MidpointRounding.AwayFromZero);
        return new QuizScoreCalculation(
            true,
            CorrectAnswerBaseScore,
            speedBonus,
            CorrectAnswerBaseScore + speedBonus,
            clampedElapsedTicks);
    }
}

public sealed record QuizScoreCalculation(
    bool IsCorrect,
    int BaseScore,
    int SpeedBonus,
    int Score,
    long ElapsedTicks);
