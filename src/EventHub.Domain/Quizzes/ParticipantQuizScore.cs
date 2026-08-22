using EventHub.Domain.Common;

namespace EventHub.Domain.Quizzes;

public sealed class ParticipantQuizScore
{
    private ParticipantQuizScore()
    {
    }

    private ParticipantQuizScore(Guid eventId, Guid quizId, Guid participantId, DateTimeOffset nowUtc)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        QuizId = quizId;
        ParticipantId = participantId;
        UpdatedAtUtc = nowUtc;
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public Guid QuizId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public int TotalScore { get; private set; }
    public int CorrectCount { get; private set; }
    public int AnsweredCount { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static ParticipantQuizScore Create(
        Guid eventId,
        Guid quizId,
        Guid participantId,
        DateTimeOffset nowUtc)
    {
        if (eventId == Guid.Empty || quizId == Guid.Empty || participantId == Guid.Empty)
        {
            throw new DomainValidationException("Quiz 累積分數的識別碼不可為空白。");
        }

        return new ParticipantQuizScore(eventId, quizId, participantId, nowUtc);
    }

    public void Apply(ParticipantQuestionResult result, DateTimeOffset nowUtc)
    {
        if (result.EventId != EventId || result.QuizId != QuizId || result.ParticipantId != ParticipantId)
        {
            throw new DomainValidationException("每題結果不屬於此 Quiz 累積分數。");
        }

        TotalScore += result.Score;
        AnsweredCount++;
        if (result.IsCorrect)
        {
            CorrectCount++;
        }

        UpdatedAtUtc = nowUtc;
    }
}
