using EventHub.Application.Quizzes;

namespace EventHub.Server.Endpoints;

public static class QuestionBankEndpoints
{
    public static IEndpointRouteBuilder MapQuestionBankEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/quiz-import/template", CreateTemplateResult);
        var group = endpoints.MapGroup("/api/v1/events/{eventId:guid}");

        group.MapPost("/quiz-import/preview", async (
            Guid eventId,
            HttpRequest request,
            QuestionBankService service,
            ILogger<QuestionBankEndpointLog> logger,
            CancellationToken cancellationToken) =>
        {
            var file = await ReadFileAsync(request, cancellationToken);
            logger.LogInformation(
                "Question bank preview started for event {EventId}, file {FileName}, size {FileSize}.",
                eventId,
                file.FileName,
                file.Length);
            await using var stream = file.OpenReadStream();
            var preview = await service.PreviewAsync(
                eventId,
                ReadHostToken(request),
                file.FileName,
                stream,
                file.Length,
                cancellationToken);
            logger.LogInformation(
                "Question bank preview completed for event {EventId}: {QuestionCount} rows, {ErrorCount} errors, {WarningCount} warnings.",
                eventId,
                preview.QuestionCount,
                preview.ErrorCount,
                preview.WarningCount);
            return Results.Ok(preview);
        }).DisableAntiforgery();

        group.MapPost("/quiz-import", async (
            Guid eventId,
            HttpRequest request,
            QuestionBankService service,
            ILogger<QuestionBankEndpointLog> logger,
            CancellationToken cancellationToken) =>
        {
            var file = await ReadFileAsync(request, cancellationToken);
            logger.LogInformation(
                "Question bank import started for event {EventId}, file {FileName}, size {FileSize}.",
                eventId,
                file.FileName,
                file.Length);
            await using var stream = file.OpenReadStream();
            QuestionBankImportResult result;
            try
            {
                result = await service.ImportAsync(
                    eventId,
                    ReadHostToken(request),
                    file.FileName,
                    stream,
                    file.Length,
                    cancellationToken);
            }
            catch (QuestionBankImportValidationException exception)
            {
                logger.LogWarning(
                    "Question bank import validation failed for event {EventId}: {ErrorCount} errors.",
                    eventId,
                    exception.Preview.ErrorCount);
                throw;
            }
            logger.LogInformation(
                "Question bank {QuizId} imported for event {EventId} with {QuestionCount} questions.",
                result.QuizId,
                eventId,
                result.QuestionCount);
            return Results.Created(
                $"/api/v1/events/{eventId:D}/quiz/question-banks/{result.QuizId:D}",
                result);
        }).DisableAntiforgery();

        group.MapGet("/quiz-import/template", async (
            Guid eventId,
            HttpRequest request,
            QuestionBankService service,
            CancellationToken cancellationToken) =>
        {
            await service.AuthorizeAsync(eventId, ReadHostToken(request), cancellationToken);
            return CreateTemplateResult();
        });

        group.MapGet("/quiz/question-banks", async (
            Guid eventId,
            HttpRequest request,
            QuestionBankService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListAsync(eventId, ReadHostToken(request), cancellationToken)));

        group.MapPost("/quiz/default-practice", async (
            Guid eventId,
            HttpRequest request,
            QuestionBankService service,
            CancellationToken cancellationToken) =>
        {
            var result = await service.EnsureDefaultPracticeAsync(
                eventId,
                ReadHostToken(request),
                cancellationToken);
            return Results.Ok(result);
        });

        group.MapGet("/quiz/question-banks/{quizId:guid}/questions", async (
            Guid eventId,
            Guid quizId,
            HttpRequest request,
            QuestionBankService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.ListQuestionsAsync(
                eventId,
                quizId,
                ReadHostToken(request),
                cancellationToken)));

        return endpoints;
    }

    private static IResult CreateTemplateResult()
    {
        return Results.File(
            QuestionBankCsvTemplate.CreateUtf8BomBytes(),
            "text/csv; charset=utf-8",
            "EventHub_QuizTemplate.csv");
    }

    private static async Task<IFormFile> ReadFileAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
        {
            throw new BadHttpRequestException("請使用 multipart/form-data 上傳 CSV。");
        }

        var form = await request.ReadFormAsync(cancellationToken);
        return form.Files.GetFile("file")
            ?? throw new BadHttpRequestException("缺少名為 file 的 CSV 檔案欄位。");
    }

    private static string ReadHostToken(HttpRequest request) =>
        request.Headers["X-Host-Token"].ToString();

    private sealed class QuestionBankEndpointLog;
}
