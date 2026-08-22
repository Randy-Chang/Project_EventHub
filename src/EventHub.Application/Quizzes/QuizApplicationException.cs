namespace EventHub.Application.Quizzes;

public enum QuizErrorCode
{
    QuestionNotFound,
    SessionNotFound,
    QuestionAlreadyOpen,
    InvalidQuestionState,
    InvalidOption
}

public sealed class QuizApplicationException(QuizErrorCode code, string message) : Exception(message)
{
    public QuizErrorCode Code { get; } = code;
}
