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
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled server exception.");
            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "伺服器發生未預期錯誤。");
        }
    }

    private static Task WriteProblemAsync(HttpContext context, int statusCode, string detail)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new
        {
            type = "about:blank",
            title = ReasonPhrases.GetReasonPhrase(statusCode),
            status = statusCode,
            detail
        });
    }
}
