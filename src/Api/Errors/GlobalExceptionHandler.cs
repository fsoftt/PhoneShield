using FluentValidation;
using Tranqui.Application.Errors;
using Microsoft.AspNetCore.Diagnostics;

namespace Tranqui.Api.Errors;

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
    private const string AccountNotRegisteredTitle = "Registra tu cuenta antes de aportar.";
    private const string ConsentRequiredTitle = "Primero acepta el permiso para aportar tus contactos.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            return await WriteValidationProblemAsync(httpContext, validationException);
        }

        if (exception is AccountNotRegisteredException)
        {
            return await WriteProblemAsync(httpContext, StatusCodes.Status409Conflict, AccountNotRegisteredTitle);
        }

        if (exception is ConsentRequiredException)
        {
            return await WriteProblemAsync(httpContext, StatusCodes.Status403Forbidden, ConsentRequiredTitle);
        }

        LogUnhandledException(exception.GetType().Name);

        return await WriteProblemAsync(httpContext, StatusCodes.Status500InternalServerError, UnexpectedErrorTitle);
    }

    private async ValueTask<bool> WriteProblemAsync(HttpContext httpContext, int statusCode, string title)
    {
        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = { Title = title, Status = statusCode },
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
