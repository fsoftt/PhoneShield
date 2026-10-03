using Tranqui.App.Core.Accounts;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Navigation;

namespace Tranqui.App.Core.ViewModels;

/// <summary>Restores the previous session on launch and sends the user to the right screen.</summary>
public sealed class StartupViewModel(
    IAuthService authService,
    IAccountService accountService,
    INavigationService navigation)
{
    public async Task InitializeAsync()
    {
        try
        {
            await navigation.GoToAsync(await ResolveRouteAsync());
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            // Offline: a restored session still works (call screening uses cached data), otherwise sign in later.
            await navigation.GoToAsync(authService.IsSignedIn ? Routes.Home : Routes.SignIn);
        }
    }

    private async Task<string> ResolveRouteAsync()
    {
        if (!await authService.RestoreAsync(CancellationToken.None))
        {
            return Routes.SignIn;
        }

        if (!await authService.IsEmailVerifiedAsync(CancellationToken.None))
        {
            return Routes.VerifyEmail;
        }

        await accountService.EnsureRegisteredAsync(CancellationToken.None);
        return Routes.Home;
    }
}
