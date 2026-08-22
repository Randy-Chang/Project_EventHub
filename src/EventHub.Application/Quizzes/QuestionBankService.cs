using EventHub.Application.Abstractions;
using EventHub.Application.Events;
using EventHub.Domain.Quizzes;

namespace EventHub.Application.Quizzes;

public sealed class QuestionBankService(
    IQuestionBankCsvParser parser,
    IQuestionBankRepository repository,
    EventService eventService,
    QuestionBankValidator validator,
    TimeProvider timeProvider)
{
    public Task AuthorizeAsync(Guid eventId, string hostToken, CancellationToken cancellationToken) =>
        EnsureAuthorizedAsync(eventId, hostToken, cancellationToken);

    public async Task<QuestionBankPreview> PreviewAsync(
        Guid eventId,
        string hostToken,
        string fileName,
        Stream stream,
        long fileLength,
        CancellationToken cancellationToken)
    {
        await EnsureAuthorizedAsync(eventId, hostToken, cancellationToken);
        var sizeIssue = ValidateFile(fileName, fileLength);
        if (sizeIssue is not null)
        {
            return new QuestionBankPreview(fileName, null, 0, [], [sizeIssue]);
        }

        var parsed = await parser.ParseAsync(stream, cancellationToken);
        var (title, rows, validationIssues) = validator.Validate(parsed);
        var issues = validationIssues.ToList();
        if (title is not null && await repository.TitleExistsAsync(eventId, title, cancellationToken))
        {
            issues.Add(new QuestionBankIssue(null, null, "QuizTitle", "此活動已存在相同名稱的題庫。"));
        }

        return new QuestionBankPreview(fileName, title, parsed.Rows.Count, rows, issues);
    }

    public async Task<QuestionBankImportResult> ImportAsync(
        Guid eventId,
        string hostToken,
        string fileName,
        Stream stream,
        long fileLength,
        CancellationToken cancellationToken)
    {
        var preview = await PreviewAsync(
            eventId,
            hostToken,
            fileName,
            stream,
            fileLength,
            cancellationToken);
        if (!preview.IsValid || preview.QuizTitle is null)
        {
            throw new QuestionBankImportValidationException(preview);
        }

        var importedAtUtc = timeProvider.GetUtcNow();
        var quiz = Quiz.Create(eventId, preview.QuizTitle, importedAtUtc);
        var questions = preview.Rows.Select(row => QuizQuestion.Create(
            quiz.Id,
            row.QuestionKey,
            row.Category,
            row.Difficulty,
            row.Question,
            row.Options,
            row.CorrectOptionIndex,
            TimeSpan.FromSeconds(row.DurationSeconds),
            row.Order)).ToArray();
        await repository.ImportAsync(quiz, questions, cancellationToken);
        return new QuestionBankImportResult(quiz.Id, quiz.Title, questions.Length, importedAtUtc);
    }

    public async Task<IReadOnlyList<QuestionBankSummary>> ListAsync(
        Guid eventId,
        string hostToken,
        CancellationToken cancellationToken)
    {
        await EnsureAuthorizedAsync(eventId, hostToken, cancellationToken);
        return (await repository.ListAsync(eventId, cancellationToken))
            .Select(item => new QuestionBankSummary(item.Id, item.Title, item.QuestionCount, item.CreatedAtUtc))
            .ToArray();
    }

    public async Task<IReadOnlyList<QuestionBankQuestionSummary>> ListQuestionsAsync(
        Guid eventId,
        Guid quizId,
        string hostToken,
        CancellationToken cancellationToken)
    {
        await EnsureAuthorizedAsync(eventId, hostToken, cancellationToken);
        if (await repository.GetAsync(eventId, quizId, cancellationToken) is null)
        {
            throw new KeyNotFoundException("找不到指定題庫。");
        }

        return (await repository.ListQuestionsAsync(eventId, quizId, cancellationToken))
            .Select(item =>
            {
                var question = item.Question;
                var options = question.Options.OrderBy(option => option.Order).ToArray();
                return new QuestionBankQuestionSummary(
                    question.Id,
                    question.QuestionKey,
                    question.Category,
                    question.Difficulty,
                    question.Order,
                    question.Text,
                    options.Select(option => new QuizOptionSummary(option.Id, option.Text, option.Order)).ToArray(),
                    Array.FindIndex(options, option => option.Id == question.CorrectOptionId),
                    (int)question.AnswerDuration.TotalSeconds,
                    item.IsCurrent
                        ? QuestionBankQuestionStatus.Current
                        : item.WasRevealed
                            ? QuestionBankQuestionStatus.Completed
                            : QuestionBankQuestionStatus.Pending);
            })
            .ToArray();
    }

    private async Task EnsureAuthorizedAsync(Guid eventId, string hostToken, CancellationToken cancellationToken)
    {
        if (!await eventService.IsHostAuthorizedAsync(eventId, hostToken, cancellationToken))
        {
            throw new UnauthorizedAccessException("Host credential 無效。");
        }

        _ = await eventService.GetAsync(eventId, cancellationToken);
    }

    private static QuestionBankIssue? ValidateFile(string fileName, long fileLength)
    {
        if (!string.Equals(Path.GetExtension(fileName), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            return new QuestionBankIssue(null, null, "File", "請選擇 .csv 檔案。");
        }

        return fileLength is <= 0 or > QuestionBankImportLimits.MaximumFileSizeBytes
            ? new QuestionBankIssue(null, null, "File", "CSV 檔案須大於 0 且不可超過 5 MB。")
            : null;
    }
}

public sealed class QuestionBankImportValidationException(QuestionBankPreview preview)
    : Exception("題庫 CSV 驗證失敗，未匯入任何資料。")
{
    public QuestionBankPreview Preview { get; } = preview;
}

public sealed class QuestionBankImportConflictException(string message) : Exception(message);
