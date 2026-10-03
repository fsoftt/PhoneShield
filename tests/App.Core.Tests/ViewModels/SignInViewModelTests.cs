using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tranqui.App.Core.Accounts;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class SignInViewModelTests
{
    private readonly IAuthService authService = Substitute.For<IAuthService>();
    private readonly IAccountService accountService = Substitute.For<IAccountService>();
    private readonly INavigationService navigation = Substitute.For<INavigationService>();
    private readonly SignInViewModel viewModel;

    public SignInViewModelTests()
    {
        viewModel = new SignInViewModel(authService, accountService, navigation)
        {
            Email = " ana@example.com ",
            Password = "secret123",
        };
    }

    [Fact]
    public async Task SignIn_WithoutEmail_ShowsErrorWithoutCallingFirebase()
    {
        viewModel.Email = string.Empty;

        await viewModel.SignInCommand.ExecuteAsync(null);

        viewModel.ErrorMessage.Should().Be(Texts.EmailRequired);
        await authService.DidNotReceive().SignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignIn_VerifiedEmail_RegistersAndGoesHome()
    {
        authService.IsEmailVerifiedAsync(Arg.Any<CancellationToken>()).Returns(true);

        await viewModel.SignInCommand.ExecuteAsync(null);

        await authService.Received(1).SignInAsync("ana@example.com", "secret123", Arg.Any<CancellationToken>());
        await accountService.Received(1).EnsureRegisteredAsync(Arg.Any<CancellationToken>());
        await navigation.Received(1).GoToAsync(Routes.Home);
        viewModel.Password.Should().BeEmpty();
    }

    [Fact]
    public async Task SignIn_UnverifiedEmail_GoesToVerification()
    {
        await viewModel.SignInCommand.ExecuteAsync(null);

        await navigation.Received(1).GoToAsync(Routes.VerifyEmail);
        await accountService.DidNotReceive().EnsureRegisteredAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignIn_FirebaseRejects_ShowsItsMessageAndStopsBeingBusy()
    {
        authService.SignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new FirebaseAuthException("INVALID_LOGIN_CREDENTIALS", "Correo o contraseña incorrectos."));

        await viewModel.SignInCommand.ExecuteAsync(null);

        viewModel.ErrorMessage.Should().Be("Correo o contraseña incorrectos.");
        viewModel.IsBusy.Should().BeFalse();
        await navigation.DidNotReceive().GoToAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task SignIn_NetworkFailure_ShowsConnectionError()
    {
        authService.SignInAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        await viewModel.SignInCommand.ExecuteAsync(null);

        viewModel.ErrorMessage.Should().Be(Texts.ConnectionError);
    }

    [Fact]
    public async Task ForgotPassword_SendsResetAndInformsTheUser()
    {
        await viewModel.ForgotPasswordCommand.ExecuteAsync(null);

        await authService.Received(1).SendPasswordResetAsync("ana@example.com", Arg.Any<CancellationToken>());
        viewModel.InfoMessage.Should().Be(Texts.PasswordResetSent);
    }
}
