using EventHub.Domain.Common;

namespace EventHub.Domain.Quizzes;

public sealed class ParticipantAnswer
{
    private ParticipantAnswer()
    {
    }

    private ParticipantAnswer(
        Guid eventId,
        Guid questionSessionId,
        Guid participantId,
        Guid selectedOptionId,
        DateTimeOffset submittedAtUtc)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        QuestionSessionId = questionSessionId;
        ParticipantId = participantId;
        SelectedOptionId = selectedOptionId;
        SubmittedAtUtc = submittedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Guid QuestionSessionId { get; private set; }

    public Guid ParticipantId { get; private set; }

    public Guid SelectedOptionId { get; private set; }

    public DateTimeOffset SubmittedAtUtc { get; private set; }

    public static ParticipantAnswer Create(
        Guid eventId,
        Guid questionSessionId,
        Guid participantId,
        Guid selectedOptionId,
        DateTimeOffset submittedAtUtc)
    {
        if (eventId == Guid.Empty || questionSessionId == Guid.Empty || participantId == Guid.Empty || selectedOptionId == Guid.Empty)
        {
            throw new DomainValidationException("答案的識別碼不可為空白。");
        }

        return new ParticipantAnswer(eventId, questionSessionId, participantId, selectedOptionId, submittedAtUtc);
    }
}
