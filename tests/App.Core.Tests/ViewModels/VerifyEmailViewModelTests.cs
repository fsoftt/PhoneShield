using NSubstitute;
using Tranqui.App.Core.Accounts;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class VerifyEmailViewModelTests
{
    private readonly IAuthService authService = Substitute.For<IAuthService>();
    private readonly IAccountService accountService = Substitute.For<IAccountService>();
    private readonly INavigationService navigation = Substitute.For<INavigationService>();
    private readonly VerifyEmailViewModel viewModel;

    public VerifyEmailViewModelTests()
    {
        viewModel = new VerifyEmailViewModel(authService, accountService, navigation);
    }

    [Fact]
    public async Task Check_NotVerifiedYet_ExplainsWhatToDo()
    {
        await viewModel.CheckCommand.ExecuteAsync(null);

        viewModel.ErrorMessage.Should().Be(Texts.EmailNotVerifiedYet);
        await navigation.DidNotReceive().GoToAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Check_Verified_RegistersTheAccountAndGoesHome()
    {
        authService.IsEmailVerifiedAsync(Arg.Any<CancellationToken>()).Returns(true);

        await viewModel.CheckCommand.ExecuteAsync(null);

        await accountService.Received(1).EnsureRegisteredAsync(Arg.Any<CancellationToken>());
        await navigation.Received(1).GoToAsync(Routes.Home);
    }

    [Fact]
    public async Task UseAnotherAccount_SignsOutAndReturnsToSignIn()
    {
        await viewModel.UseAnotherAccountCommand.ExecuteAsync(null);

        authService.Received(1).SignOut();
        await navigation.Received(1).GoToAsync(Routes.SignIn);
    }
}
