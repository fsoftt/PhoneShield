using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Tranqui.App.Core.Accounts;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class StartupViewModelTests
{
    private readonly IAuthService authService = Substitute.For<IAuthService>();
    private readonly IAccountService accountService = Substitute.For<IAccountService>();
    private readonly INavigationService navigation = Substitute.For<INavigationService>();
    private readonly StartupViewModel viewModel;

    public StartupViewModelTests()
    {
        viewModel = new StartupViewModel(authService, accountService, navigation);
    }

    [Fact]
    public async Task Initialize_NoPreviousSession_GoesToSignIn()
    {
        await viewModel.InitializeAsync();

        await navigation.Received(1).GoToAsync(Routes.SignIn);
    }

    [Fact]
    public async Task Initialize_UnverifiedEmail_GoesToVerification()
    {
        authService.RestoreAsync(Arg.Any<CancellationToken>()).Returns(true);

        await viewModel.InitializeAsync();

        await navigation.Received(1).GoToAsync(Routes.VerifyEmail);
    }

    [Fact]
    public async Task Initialize_VerifiedSession_EnsuresTheAccountAndGoesHome()
    {
        authService.RestoreAsync(Arg.Any<CancellationToken>()).Returns(true);
        authService.IsEmailVerifiedAsync(Arg.Any<CancellationToken>()).Returns(true);

        await viewModel.InitializeAsync();

        await accountService.Received(1).EnsureRegisteredAsync(Arg.Any<CancellationToken>());
        await navigation.Received(1).GoToAsync(Routes.Home);
    }

    [Fact]
    public async Task Initialize_OfflineWithRestoredSession_StillGoesHome()
    {
        authService.RestoreAsync(Arg.Any<CancellationToken>()).Returns(true);
        authService.IsSignedIn.Returns(true);
        authService.IsEmailVerifiedAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new HttpRequestException());

        await viewModel.InitializeAsync();

        await navigation.Received(1).GoToAsync(Routes.Home);
    }
}
