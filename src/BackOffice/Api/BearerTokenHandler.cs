using System.Net.Http.Headers;
using Tranqui.BackOffice.Authentication;

namespace Tranqui.BackOffice.Api;

/// <summary>Sends the signed-in admin's Firebase ID token (prepared by <see cref="SessionTokenMiddleware"/>) to the API.</summary>
internal sealed class BearerTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null
            && httpContextAccessor.HttpContext?.Items[SessionTokenMiddleware.IdTokenItem] is string idToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
