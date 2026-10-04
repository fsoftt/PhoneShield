using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Tranqui.BackOffice.Authentication;

/// <summary>
/// Keeps the Firebase tokens inside the encrypted, HTTP-only authentication cookie: the browser never sees them and
/// no JavaScript can read them.
/// </summary>
internal static class SessionCookie
{
    public const string Scheme = CookieAuthenticationDefaults.AuthenticationScheme;
    public const string Name = "__Host-tranqui-backoffice";

    private const string IdTokenName = "id_token";
    private const string RefreshTokenName = "refresh_token";
    private const string ExpiresAtName = "expires_at";

    public static Task SignInAsync(HttpContext httpContext, FirebaseSession session)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, session.Uid), new Claim(ClaimTypes.Email, session.Email)],
            Scheme);
        var properties = new AuthenticationProperties { IsPersistent = false };
        Store(properties, session);

        return httpContext.SignInAsync(Scheme, new ClaimsPrincipal(identity), properties);
    }

    public static FirebaseSession? Read(AuthenticateResult result)
    {
        if (!result.Succeeded
            || result.Properties.GetTokenValue(IdTokenName) is not { } idToken
            || result.Properties.GetTokenValue(RefreshTokenName) is not { } refreshToken
            || !DateTimeOffset.TryParse(result.Properties.GetTokenValue(ExpiresAtName), CultureInfo.InvariantCulture, out var expiresAt))
        {
            return null;
        }

        var user = result.Principal;

        return new FirebaseSession(
            user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            idToken,
            refreshToken,
            expiresAt);
    }

    public static Task RenewAsync(HttpContext httpContext, AuthenticateResult result, FirebaseSession session)
    {
        ArgumentNullException.ThrowIfNull(result);
        Store(result.Properties!, session);

        return httpContext.SignInAsync(Scheme, result.Principal!, result.Properties);
    }

    private static void Store(AuthenticationProperties properties, FirebaseSession session) =>
        properties.StoreTokens(
        [
            new AuthenticationToken { Name = IdTokenName, Value = session.IdToken },
            new AuthenticationToken { Name = RefreshTokenName, Value = session.RefreshToken },
            new AuthenticationToken { Name = ExpiresAtName, Value = session.ExpiresAt.ToString("O", CultureInfo.InvariantCulture) },
        ]);
}
