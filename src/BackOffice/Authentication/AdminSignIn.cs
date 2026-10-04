using Tranqui.BackOffice.Api;

namespace Tranqui.BackOffice.Authentication;

public enum SignInOutcome
{
    SignedIn,
    WrongCredentials,
    NotAnAdmin,
    Unavailable,
}

/// <summary>
/// Signs in with Firebase, then asks the API whether the account is an admin before creating the session: an account
/// that is not on the API's admin list never gets a back office cookie.
/// </summary>
public sealed class AdminSignIn(IFirebasePasswordAuth firebase, AdminApiClient api)
{
    public async Task<SignInOutcome> SignInAsync(HttpContext httpContext, string email, string password)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        try
        {
            var session = await firebase.SignInAsync(email, password, httpContext.RequestAborted);
            if (session is null)
            {
                return SignInOutcome.WrongCredentials;
            }

            await api.GetOverviewAsync(session.IdToken, httpContext.RequestAborted);
            await SessionCookie.SignInAsync(httpContext, session);

            return SignInOutcome.SignedIn;
        }
        catch (AdminApiException exception) when (exception.IsForbidden)
        {
            return SignInOutcome.NotAnAdmin;
        }
        catch (Exception exception) when (exception is HttpRequestException or AdminApiException or TaskCanceledException)
        {
            return SignInOutcome.Unavailable;
        }
    }
}
