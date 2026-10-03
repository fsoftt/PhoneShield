using NSubstitute;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class SignUpViewModelTests
{
    private readonly IAuthService authService = Substitute.For<IAuthService>();
    private readonly INavigationService navigation = Substitute.For<INavigationService>();
    private readonly SignUpViewModel viewModel;

    public SignUpViewModelTests()
    {
        viewModel = new SignUpViewModel(authService, navigation)
        {
            Email = "ana@example.com",
            Password = "secret123",
            ConfirmPassword = "secret123",
            AcceptedTerms = true,
        };
    }

    [Fact]
    public async Task SignUp_ShortPassword_ShowsError()
    {
        viewModel.Password = viewModel.ConfirmPassword = "short";

        await viewModel.SignUpCommand.ExecuteAsync(null);

        viewModel.ErrorMessage.Should().Be(Texts.PasswordTooShort);
    }

    [Fact]
    public async Task SignUp_PasswordsDiffer_ShowsError()
    {
        viewModel.ConfirmPassword = "different1";

        await viewModel.SignUpCommand.ExecuteAsync(null);

        viewModel.ErrorMessage.Should().Be(Texts.PasswordsDoNotMatch);
    }

    [Fact]
    public async Task SignUp_TermsNotAccepted_ShowsErrorWithoutCreatingTheAccount()
    {
        viewModel.AcceptedTerms = false;

        await viewModel.SignUpCommand.ExecuteAsync(null);

        viewModel.ErrorMessage.Should().Be(Texts.TermsRequired);
        await authService.DidNotReceive().SignUpAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignUp_Valid_CreatesTheAccountAndAsksToVerifyTheEmail()
    {
        await viewModel.SignUpCommand.ExecuteAsync(null);

        await authService.Received(1).SignUpAsync("ana@example.com", "secret123", Arg.Any<CancellationToken>());
        await navigation.Received(1).GoToAsync(Routes.VerifyEmail);
        viewModel.Password.Should().BeEmpty();
    }
}
