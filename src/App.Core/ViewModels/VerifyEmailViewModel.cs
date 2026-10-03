using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Accounts;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.ViewModels;

public sealed partial class VerifyEmailViewModel(
    IAuthService authService,
    IAccountService accountService,
    INavigationService navigation) : FormViewModel
{
    public string? Email => authService.Email;

    [RelayCommand]
    private Task CheckAsync() => RunAsync(async () =>
    {
        if (!await authService.IsEmailVerifiedAsync(CancellationToken.None))
        {
            ErrorMessage = Texts.EmailNotVerifiedYet;
            return;
        }

        await accountService.EnsureRegisteredAsync(CancellationToken.None);
        await navigation.GoToAsync(Routes.Home);
    });

    [RelayCommand]
    private Task ResendAsync() => RunAsync(async () =>
    {
        await authService.ResendEmailVerificationAsync(CancellationToken.None);
        InfoMessage = Texts.VerificationSent;
    });

    [RelayCommand]
    private Task UseAnotherAccountAsync()
    {
        authService.SignOut();
        return navigation.GoToAsync(Routes.SignIn);
    }
}
