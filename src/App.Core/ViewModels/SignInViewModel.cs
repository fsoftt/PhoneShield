using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Accounts;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.ViewModels;

public sealed partial class SignInViewModel(
    IAuthService authService,
    IAccountService accountService,
    INavigationService navigation) : FormViewModel
{
    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [RelayCommand]
    private Task SignInAsync()
    {
        if (!Require(!string.IsNullOrWhiteSpace(Email), Texts.EmailRequired)
            || !Require(!string.IsNullOrEmpty(Password), Texts.PasswordRequired))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await authService.SignInAsync(Email.Trim(), Password, CancellationToken.None);
            Password = string.Empty;

            if (!await authService.IsEmailVerifiedAsync(CancellationToken.None))
            {
                await navigation.GoToAsync(Routes.VerifyEmail);
                return;
            }

            await accountService.EnsureRegisteredAsync(CancellationToken.None);
            await navigation.GoToAsync(Routes.Home);
        });
    }

    [RelayCommand]
    private Task ForgotPasswordAsync()
    {
        if (!Require(!string.IsNullOrWhiteSpace(Email), Texts.EmailRequired))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await authService.SendPasswordResetAsync(Email.Trim(), CancellationToken.None);
            InfoMessage = Texts.PasswordResetSent;
        });
    }

    [RelayCommand]
    private Task GoToSignUpAsync() => navigation.GoToAsync(Routes.SignUp);
}
