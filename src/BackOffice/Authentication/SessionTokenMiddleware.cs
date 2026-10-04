using Microsoft.AspNetCore.Authentication;

namespace Tranqui.BackOffice.Authentication;

/// <summary>
/// Before each request: renews the Firebase ID token when it is about to expire (and the cookie with it), and makes the
/// current token available to <see cref="Api.BearerTokenHandler"/>. An expired session signs the user out.
/// </summary>
internal sealed class SessionTokenMiddleware(RequestDelegate next, IFirebasePasswordAuth firebase, TimeProvider timeProvider)
{
    public const string IdTokenItem = "tranqui.id_token";

    private static readonly TimeSpan renewalMargin = TimeSpan.FromMinutes(5);

    public async Task InvokeAsync(HttpContext context)
    {
        var result = await context.AuthenticateAsync(SessionCookie.Scheme);
        if (SessionCookie.Read(result) is { } session)
        {
            if (session.ExpiresAt - timeProvider.GetUtcNow() < renewalMargin)
            {
                var renewed = await firebase.RefreshAsync(session, context.RequestAborted);
                if (renewed is null)
                {
                    await context.SignOutAsync(SessionCookie.Scheme);
                    context.Response.Redirect(BackOfficeRoutes.Login);
                    return;
                }

                await SessionCookie.RenewAsync(context, result, renewed);
                session = renewed;
            }

            context.Items[IdTokenItem] = session.IdToken;
        }

        await next(context);
    }
}
