using NSubstitute;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Contacts;
using Tranqui.App.Core.Protection;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.ViewModels;
using Tranqui.Contracts.Contacts;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class SettingsViewModelTests
{
    private readonly IScreeningSettingsStore store = Substitute.For<IScreeningSettingsStore>();
    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly IContributionState contributionState = Substitute.For<IContributionState>();
    private readonly IProtectionPermissions permissions = Substitute.For<IProtectionPermissions>();

    public SettingsViewModelTests()
    {
        store.Load().Returns(ScreeningSettings.Default);
    }

    [Fact]
    public void Constructor_ShowsTheSavedSettings()
    {
        store.Load().Returns(new ScreeningSettings(BlockCommunitySpam: true, BlockPrivateNumbers: false, BlockInternational: true));

        var viewModel = CreateViewModel();

        viewModel.BlockCommunitySpam.Should().BeTrue();
        viewModel.BlockPrivateNumbers.Should().BeFalse();
        viewModel.BlockInternational.Should().BeTrue();
    }

    [Fact]
    public void ChangingAnOption_SavesAllOptions()
    {
        var viewModel = CreateViewModel();

        viewModel.BlockPrivateNumbers = true;

        store.Received(1).Save(ScreeningSettings.Default with { BlockPrivateNumbers = true });
    }

    [Fact]
    public async Task StartContributing_WithoutContactsPermission_ExplainsAndDoesNotUpload()
    {
        var viewModel = CreateViewModel();

        await viewModel.StartContributingCommand.ExecuteAsync(null);

        await permissions.Received(1).RequestReadContactsAsync();
        viewModel.ErrorMessage.Should().Be(Texts.ContactsPermissionNeeded);
        viewModel.IsContributing.Should().BeFalse();
    }

    [Fact]
    public async Task StartContributing_WithPermission_ContributesAndThanksTheUser()
    {
        permissions.CanReadContacts.Returns(true);
        var viewModel = CreateViewModel();

        await viewModel.StartContributingCommand.ExecuteAsync(null);

        viewModel.IsContributing.Should().BeTrue();
        viewModel.InfoMessage.Should().Be(Texts.Format(Texts.ContributionStartedFormat, 0));
    }

    [Fact]
    public async Task StopContributing_WithdrawsAndConfirms()
    {
        contributionState.IsContributing.Returns(true);
        var viewModel = CreateViewModel();

        await viewModel.StopContributingCommand.ExecuteAsync(null);

        await api.Received(1).WithdrawContactsAsync(Arg.Any<CancellationToken>());
        viewModel.IsContributing.Should().BeFalse();
        viewModel.InfoMessage.Should().Be(Texts.ContributionStopped);
    }

    private SettingsViewModel CreateViewModel()
    {
        var contacts = Substitute.For<IDeviceContactSource>();
        contacts.ReadAll().Returns([]);
        api.WithdrawContactsAsync(Arg.Any<CancellationToken>()).Returns(new WithdrawContactsResponse(0));
        var contribution = new ContactContributionService(api, contacts, contributionState, TimeProvider.System);

        return new SettingsViewModel(store, contribution, permissions);
    }
}
