using FluentValidation;
using Tranqui.Application.Errors;
using Tranqui.Contracts.Errors;
using Microsoft.AspNetCore.Diagnostics;

namespace Tranqui.Api.Errors;

/// <summary>
/// Maps validation failures to a 400 with per-field errors and anything else to a generic 500. Responses are in
/// English with a stable <see cref="ApiErrorCodes"/> code; the app shows its own localized message for each code.
/// Exception messages are never returned to clients, so no PII can leak through error responses.
/// </summary>
public sealed partial class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private const string UnexpectedErrorTitle = "An unexpected error occurred.";
    private const string ValidationErrorTitle = "The request is not valid.";
    private const string AccountNotRegisteredTitle = "The account must be registered first.";
    private const string ConsentRequiredTitle = "The contact upload consent is required.";

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
            return await WriteProblemAsync(httpContext, StatusCodes.Status409Conflict, AccountNotRegisteredTitle, ApiErrorCodes.AccountNotRegistered);
        }

        if (exception is ConsentRequiredException)
        {
            return await WriteProblemAsync(httpContext, StatusCodes.Status403Forbidden, ConsentRequiredTitle, ApiErrorCodes.ConsentRequired);
        }

        LogUnhandledException(exception.GetType().Name);

        return await WriteProblemAsync(httpContext, StatusCodes.Status500InternalServerError, UnexpectedErrorTitle, ApiErrorCodes.UnexpectedError);
    }

    private async ValueTask<bool> WriteProblemAsync(HttpContext httpContext, int statusCode, string title, string code)
    {
        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = { Title = title, Status = statusCode, Extensions = { [ApiErrorCodes.Field] = code } },
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
                Extensions = { [ApiErrorCodes.Field] = ApiErrorCodes.ValidationFailed },
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception of type {ExceptionType}")]
    private partial void LogUnhandledException(string exceptionType);
}
