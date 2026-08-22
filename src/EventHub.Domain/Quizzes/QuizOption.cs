namespace EventHub.Domain.Quizzes;

public sealed class QuizOption
{
    private QuizOption()
    {
    }

    internal QuizOption(Guid questionId, string text, int order)
    {
        Id = Guid.NewGuid();
        QuizQuestionId = questionId;
        Text = text;
        Order = order;
    }

    public Guid Id { get; private set; }

    public Guid QuizQuestionId { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public int Order { get; private set; }
}
