using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace PhoneShield.Api.Errors;

/// <summary>
/// Maps validation failures to a 400 with per-field errors and anything else to a generic 500.
/// Exception messages are never returned to clients, so no PII can leak through error responses.
/// </summary>
public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string UnexpectedErrorTitle = "Ocurrió un error inesperado.";
    private const string ValidationErrorTitle = "La solicitud no es válida.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            return await WriteValidationProblemAsync(httpContext, validationException);
        }

        LogUnhandledException(exception.GetType().Name);
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = { Title = UnexpectedErrorTitle, Status = StatusCodes.Status500InternalServerError },
        });
    }

    private async ValueTask<bool> WriteValidationProblemAsync(HttpContext httpContext, ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new HttpValidationProblemDetails(errors)
            {
                Title = ValidationErrorTitle,
                Status = StatusCodes.Status400BadRequest,
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception of type {ExceptionType}")]
    private partial void LogUnhandledException(string exceptionType);
}
