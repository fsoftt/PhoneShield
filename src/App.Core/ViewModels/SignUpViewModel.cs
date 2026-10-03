using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.ViewModels;

public sealed partial class SignUpViewModel(IAuthService authService, INavigationService navigation) : FormViewModel
{
    public const int MinimumPasswordLength = 8;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Terms and the data processing policy; the account is registered with this version once the email is verified.</summary>
    [ObservableProperty]
    public partial bool AcceptedTerms { get; set; }

    [RelayCommand]
    private Task SignUpAsync()
    {
        if (!Require(!string.IsNullOrWhiteSpace(Email), Texts.EmailRequired)
            || !Require(Password.Length >= MinimumPasswordLength, Texts.PasswordTooShort)
            || !Require(Password == ConfirmPassword, Texts.PasswordsDoNotMatch)
            || !Require(AcceptedTerms, Texts.TermsRequired))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await authService.SignUpAsync(Email.Trim(), Password, CancellationToken.None);
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            await navigation.GoToAsync(Routes.VerifyEmail);
        });
    }
}
