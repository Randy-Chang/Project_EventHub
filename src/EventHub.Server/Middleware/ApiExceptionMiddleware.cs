using EventHub.Application.Quizzes;
using EventHub.Application.Events;
using EventHub.Domain.Common;
using Microsoft.AspNetCore.WebUtilities;

namespace EventHub.Server.Middleware;

public sealed class ApiExceptionMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, exception.Message);
        }
        catch (DomainValidationException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status404NotFound, exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, exception.Message);
        }
        catch (QuizApplicationException exception)
        {
            logger.LogWarning(
                "Quiz request rejected with {ErrorCode}: {Reason}",
                exception.Code,
                exception.Message);
            var statusCode = exception.Code switch
            {
                QuizErrorCode.QuestionNotFound or QuizErrorCode.SessionNotFound => StatusCodes.Status404NotFound,
                QuizErrorCode.InvalidOption => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status409Conflict
            };
            await WriteProblemAsync(context, statusCode, exception.Message, exception.Code.ToString());
        }
        catch (EventJoinCodeGenerationException exception)
        {
            logger.LogError(exception, "Event join code generation exhausted all retries.");
            await WriteProblemAsync(
                context,
                StatusCodes.Status503ServiceUnavailable,
                exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled server exception.");
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "伺服器發生未預期錯誤。");
        }
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string detail,
        string? code = null)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new
        {
            type = "about:blank",
            title = ReasonPhrases.GetReasonPhrase(statusCode),
            status = statusCode,
            detail,
            code
        });
    }
}
