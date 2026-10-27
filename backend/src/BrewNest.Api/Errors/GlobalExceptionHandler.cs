using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BrewNest.Api.Errors;

internal sealed partial class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "An unhandled exception occurred. Trace identifier: {TraceId}")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception, string traceId);

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var invalidRequest = exception is BadHttpRequestException;
        if (!invalidRequest)
        {
            LogUnhandledException(logger, exception, httpContext.TraceIdentifier);
        }

        var problemDetails = new ProblemDetails
        {
            Status = invalidRequest ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError,
            Title = invalidRequest ? "Invalid request." : "An unexpected error occurred.",
            Type = invalidRequest ? "https://httpstatuses.com/400" : "https://httpstatuses.com/500",
            Detail = invalidRequest ? "The request body or parameters could not be read." :
                "The request could not be completed. Use the trace identifier when contacting support.",
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
        return true;
    }
}
