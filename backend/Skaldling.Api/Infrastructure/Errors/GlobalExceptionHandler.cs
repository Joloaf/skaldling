using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Skaldling.Api.Infrastructure.Errors;

// Catches unhandled exceptions and converts them to ProblemDetails responses.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            BadHttpRequestException badRequest =>
                (StatusCodes.Status400BadRequest, "The request body could not be processed."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception (status {Status})", statusCode);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception (status {Status}): {Title}", statusCode, title);
        }

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = httpContext.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true
                    ? exception.Message
                    : null,
                Type = $"https://tools.ietf.org/html/rfc9110#section-15.{(statusCode >= 500 ? 6 : 5)}.{statusCode - (statusCode >= 500 ? 500 : 400) + 1}"
            }
        });
    }
}