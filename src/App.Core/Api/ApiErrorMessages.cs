using System.Net;
using System.Text.Json;
using Refit;
using Tranqui.App.Core.Resources;
using Tranqui.Contracts.Errors;

namespace Tranqui.App.Core.Api;

/// <summary>
/// The API answers in English with a stable error code; this turns that code (or, failing that, the HTTP status)
/// into a message in the phone's language.
/// </summary>
public static class ApiErrorMessages
{
    public static string For(ApiException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return CodeOf(exception) switch
        {
            ApiErrorCodes.ValidationFailed => Texts.ErrorInvalidRequest,
            ApiErrorCodes.AccountNotRegistered => Texts.ErrorAccountNotRegistered,
            ApiErrorCodes.ConsentRequired => Texts.ErrorConsentRequired,
            ApiErrorCodes.RateLimited => Texts.ErrorTooManyRequests,
            ApiErrorCodes.UnexpectedError => Texts.ErrorUnexpected,
            _ => ForStatus(exception.StatusCode),
        };
    }

    private static string ForStatus(HttpStatusCode status) => status switch
    {
        HttpStatusCode.BadRequest => Texts.ErrorInvalidRequest,
        HttpStatusCode.Unauthorized => Texts.ErrorSessionExpired,
        HttpStatusCode.TooManyRequests => Texts.ErrorTooManyRequests,
        >= HttpStatusCode.InternalServerError => Texts.ErrorUnexpected,
        _ => Texts.ConnectionError,
    };

    private static string? CodeOf(ApiException exception)
    {
        if (string.IsNullOrWhiteSpace(exception.Content))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(exception.Content);
            return document.RootElement.TryGetProperty(ApiErrorCodes.Field, out var code) ? code.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
