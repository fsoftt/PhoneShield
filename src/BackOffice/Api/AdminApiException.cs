using System.Net;

namespace Tranqui.BackOffice.Api;

/// <summary>The API refused or failed a back office request.</summary>
public sealed class AdminApiException(HttpStatusCode statusCode)
    : Exception($"The Tranqui API answered {(int)statusCode}.")
{
    public HttpStatusCode StatusCode { get; } = statusCode;

    public bool IsForbidden => StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized;
}
