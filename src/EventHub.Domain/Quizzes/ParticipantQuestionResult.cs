using EventHub.Domain.Common;

namespace EventHub.Domain.Quizzes;

public sealed class ParticipantQuestionResult
{
    private ParticipantQuestionResult()
    {
    }

    private ParticipantQuestionResult(
        Guid eventId,
        Guid quizId,
        Guid questionSessionId,
        Guid participantId,
        QuizScoreCalculation calculation,
        DateTimeOffset calculatedAtUtc)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        QuizId = quizId;
        QuestionSessionId = questionSessionId;
        ParticipantId = participantId;
        IsCorrect = calculation.IsCorrect;
        BaseScore = calculation.BaseScore;
        SpeedBonus = calculation.SpeedBonus;
        Score = calculation.Score;
        ElapsedTicks = calculation.ElapsedTicks;
        CalculatedAtUtc = calculatedAtUtc;
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid QuizId { get; private set; }
    public Guid QuestionSessionId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public bool IsCorrect { get; private set; }
    public int BaseScore { get; private set; }
    public int SpeedBonus { get; private set; }
    public int Score { get; private set; }
    public long ElapsedTicks { get; private set; }
    public DateTimeOffset CalculatedAtUtc { get; private set; }

    public static ParticipantQuestionResult Create(
        Guid eventId,
        Guid quizId,
        Guid questionSessionId,
        Guid participantId,
        QuizScoreCalculation calculation,
        DateTimeOffset calculatedAtUtc)
    {
        if (eventId == Guid.Empty || quizId == Guid.Empty || questionSessionId == Guid.Empty || participantId == Guid.Empty)
        {
            throw new DomainValidationException("計分結果的識別碼不可為空白。");
        }

        return new ParticipantQuestionResult(
            eventId,
            quizId,
            questionSessionId,
            participantId,
            calculation,
            calculatedAtUtc);
    }
}
