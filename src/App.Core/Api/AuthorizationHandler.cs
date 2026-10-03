using System.Net.Http.Headers;
using Tranqui.App.Core.Authentication;

namespace Tranqui.App.Core.Api;

/// <summary>Attaches a fresh Firebase ID token to every API request.</summary>
internal sealed class AuthorizationHandler(IAuthService authService) : DelegatingHandler
{
    private const string BearerScheme = "Bearer";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var idToken = await authService.GetIdTokenAsync(cancellationToken);
        if (idToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, idToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
