using EventHub.Domain.Common;

namespace EventHub.Domain.Quizzes;

public sealed class QuizQuestion
{
    private readonly List<QuizOption> _options = [];

    private QuizQuestion()
    {
    }

    private QuizQuestion(
        Guid id,
        Guid quizId,
        string questionKey,
        string? category,
        QuizQuestionDifficulty difficulty,
        string text,
        TimeSpan answerDuration,
        int order)
    {
        Id = id;
        QuizId = quizId;
        QuestionKey = questionKey;
        Category = category;
        Difficulty = difficulty;
        Text = text;
        AnswerDuration = answerDuration;
        Order = order;
    }

    public Guid Id { get; private set; }

    public Guid QuizId { get; private set; }

    public string QuestionKey { get; private set; } = string.Empty;

    public string? Category { get; private set; }

    public QuizQuestionDifficulty Difficulty { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public TimeSpan AnswerDuration { get; private set; }

    public int Order { get; private set; }

    public Guid CorrectOptionId { get; private set; }

    public IReadOnlyCollection<QuizOption> Options => _options.AsReadOnly();

    public static QuizQuestion Create(
        Guid quizId,
        string text,
        IReadOnlyCollection<string> optionTexts,
        int correctOptionIndex,
        TimeSpan answerDuration,
        int order)
    {
        return Create(
            quizId,
            $"MANUAL-{Guid.NewGuid():N}",
            null,
            QuizQuestionDifficulty.Medium,
            text,
            optionTexts,
            correctOptionIndex,
            answerDuration,
            order);
    }

    public static QuizQuestion Create(
        Guid quizId,
        string questionKey,
        string? category,
        QuizQuestionDifficulty difficulty,
        string text,
        IReadOnlyCollection<string> optionTexts,
        int correctOptionIndex,
        TimeSpan answerDuration,
        int order)
    {
        if (quizId == Guid.Empty)
        {
            throw new DomainValidationException("Quiz 識別碼不可為空白。");
        }

        var normalizedQuestionKey = questionKey.Trim();
        if (string.IsNullOrWhiteSpace(normalizedQuestionKey) || normalizedQuestionKey.Length > 50)
        {
            throw new DomainValidationException("題目代碼須為 1 至 50 個字元。");
        }

        var normalizedCategory = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        if (normalizedCategory?.Length > 100)
        {
            throw new DomainValidationException("題目分類不可超過 100 個字元。");
        }

        var normalizedText = text.Trim();
        if (string.IsNullOrWhiteSpace(normalizedText) || normalizedText.Length > 500)
        {
            throw new DomainValidationException("題目須為 1 至 500 個字元。");
        }

        if (optionTexts.Count is < 2 or > 4)
        {
            throw new DomainValidationException("單選題須有 2 至 4 個選項。");
        }

        if (correctOptionIndex < 0 || correctOptionIndex >= optionTexts.Count)
        {
            throw new DomainValidationException("正確答案不在選項範圍內。");
        }

        if (answerDuration < TimeSpan.FromSeconds(3) || answerDuration > TimeSpan.FromMinutes(5))
        {
            throw new DomainValidationException("作答時間須介於 3 秒至 5 分鐘。");
        }

        if (order < 1)
        {
            throw new DomainValidationException("題目順序必須大於零。");
        }

        var question = new QuizQuestion(
            Guid.NewGuid(),
            quizId,
            normalizedQuestionKey,
            normalizedCategory,
            difficulty,
            normalizedText,
            answerDuration,
            order);
        var normalizedOptions = optionTexts.Select(value => value.Trim()).ToArray();
        if (normalizedOptions.Any(string.IsNullOrWhiteSpace) || normalizedOptions.Any(value => value.Length > 200))
        {
            throw new DomainValidationException("選項須為 1 至 200 個字元。");
        }

        if (normalizedOptions.Distinct(StringComparer.OrdinalIgnoreCase).Count() != normalizedOptions.Length)
        {
            throw new DomainValidationException("選項內容不可重複。");
        }

        for (var index = 0; index < normalizedOptions.Length; index++)
        {
            question._options.Add(new QuizOption(question.Id, normalizedOptions[index], index));
        }

        question.CorrectOptionId = question._options[correctOptionIndex].Id;
        return question;
    }

    public bool ContainsOption(Guid optionId) => _options.Any(option => option.Id == optionId);
}
