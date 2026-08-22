using EventHub.Domain.Common;

namespace EventHub.Domain.Quizzes;

public sealed class QuizQuestionSession
{
    private QuizQuestionSession()
    {
    }

    private QuizQuestionSession(Guid eventId, Guid quizId, Guid questionId)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        QuizId = quizId;
        QuestionId = questionId;
        State = QuizQuestionState.Waiting;
        Version = 1;
    }

    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Guid QuizId { get; private set; }

    public Guid QuestionId { get; private set; }

    public QuizQuestionState State { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? AnswerDeadlineUtc { get; private set; }

    public DateTimeOffset? ClosedAtUtc { get; private set; }

    public DateTimeOffset? RevealedAtUtc { get; private set; }

    public long Version { get; private set; }

    public static QuizQuestionSession Create(Guid eventId, Guid quizId, Guid questionId)
    {
        if (eventId == Guid.Empty || quizId == Guid.Empty || questionId == Guid.Empty)
        {
            throw new DomainValidationException("題目場次的識別碼不可為空白。");
        }

        return new QuizQuestionSession(eventId, quizId, questionId);
    }

    public void Open(DateTimeOffset nowUtc, TimeSpan answerDuration)
    {
        if (State != QuizQuestionState.Waiting)
        {
            throw new DomainValidationException("只有等待中的題目可以開始作答。");
        }

        StartedAtUtc = nowUtc;
        AnswerDeadlineUtc = nowUtc.Add(answerDuration);
        State = QuizQuestionState.Open;
        Version++;
    }

    public bool CloseIfDeadlinePassed(DateTimeOffset nowUtc)
    {
        if (State != QuizQuestionState.Open || AnswerDeadlineUtc is null || nowUtc < AnswerDeadlineUtc.Value)
        {
            return false;
        }

        Close(nowUtc);
        return true;
    }

    public void Close(DateTimeOffset nowUtc)
    {
        if (State != QuizQuestionState.Open)
        {
            throw new DomainValidationException("只有開放作答中的題目可以關閉。");
        }

        ClosedAtUtc = nowUtc;
        State = QuizQuestionState.Closed;
        Version++;
    }

    public void Reveal(DateTimeOffset nowUtc)
    {
        if (State != QuizQuestionState.Closed)
        {
            throw new DomainValidationException("只有已關閉的題目可以公布答案。");
        }

        RevealedAtUtc = nowUtc;
        State = QuizQuestionState.Revealed;
        Version++;
    }
}
