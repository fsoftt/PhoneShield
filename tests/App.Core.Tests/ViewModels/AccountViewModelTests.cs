using NSubstitute;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Dialogs;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.ViewModels;
using Tranqui.Contracts.Accounts;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class AccountViewModelTests
{
    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly IAuthService authService = Substitute.For<IAuthService>();
    private readonly IDialogService dialogs = Substitute.For<IDialogService>();
    private readonly INavigationService navigation = Substitute.For<INavigationService>();
    private readonly AccountViewModel viewModel;

    public AccountViewModelTests()
    {
        viewModel = new AccountViewModel(api, authService, dialogs, navigation);
    }

    [Fact]
    public async Task ShowMyData_SummarizesWhatTheServerKeeps()
    {
        api.ExportMyDataAsync(Arg.Any<CancellationToken>()).Returns(
            new MyDataResponse(Guid.NewGuid(), DateTimeOffset.UtcNow, [], SpamReportCount: 4, ContactContributionCount: 120, BlockCount: 7));

        await viewModel.ShowMyDataCommand.ExecuteAsync(null);

        viewModel.MyDataSummary.Should().Contain("4").And.Contain("120");
    }

    [Fact]
    public async Task OpenAppeal_GoesToTheAppealPage()
    {
        await viewModel.OpenAppealCommand.ExecuteAsync(null);

        await navigation.Received(1).GoToAsync(Routes.Appeal);
    }

    [Fact]
    public async Task DeleteAccount_NotConfirmed_DoesNothing()
    {
        await viewModel.DeleteAccountCommand.ExecuteAsync(null);

        await api.DidNotReceive().DeleteAccountAsync(Arg.Any<CancellationToken>());
        await authService.DidNotReceive().DeleteIdentityAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAccount_Confirmed_DeletesServerDataThenIdentityAndSignsOut()
    {
        dialogs.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await viewModel.DeleteAccountCommand.ExecuteAsync(null);

        Received.InOrder(() =>
        {
            api.DeleteAccountAsync(Arg.Any<CancellationToken>());
            authService.DeleteIdentityAsync(Arg.Any<CancellationToken>());
            navigation.GoToAsync(Routes.SignIn);
        });
    }

    [Fact]
    public async Task SignOut_ReturnsToSignIn()
    {
        await viewModel.SignOutCommand.ExecuteAsync(null);

        authService.Received(1).SignOut();
        await navigation.Received(1).GoToAsync(Routes.SignIn);
    }
}
