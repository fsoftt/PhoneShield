using NSubstitute;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Appearance;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Contacts;
using Tranqui.App.Core.Protection;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.Sync;
using Tranqui.App.Core.Tests.Sync;
using Tranqui.App.Core.ViewModels;
using Tranqui.Contracts.Contacts;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class SettingsViewModelTests : IDisposable
{
    private readonly IScreeningSettingsStore store = Substitute.For<IScreeningSettingsStore>();
    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly IContributionState contributionState = Substitute.For<IContributionState>();
    private readonly IProtectionPermissions permissions = Substitute.For<IProtectionPermissions>();
    private readonly IBlockList blockList = Substitute.For<IBlockList>();
    private readonly IThemeService themes = Substitute.For<IThemeService>();
    private readonly InMemoryOutboxStore outboxStore = new();
    private readonly Outbox outbox;

    public SettingsViewModelTests()
    {
        store.Load().Returns(ScreeningSettings.Default);
        api.UnblockAsync(Arg.Any<Contracts.Blocks.BlockRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new HttpRequestException()));
        outbox = new Outbox(outboxStore, api, Substitute.For<IBackgroundSync>(), TimeProvider.System);
    }

    public void Dispose() => outbox.Dispose();

    [Fact]
    public void Constructor_ShowsTheSavedSettings()
    {
        store.Load().Returns(ScreeningSettings.Default with { BlockCommunitySpam = true, BlockInternational = true, BlockedPrefixes = ["+57601"] });

        var viewModel = CreateViewModel();

        viewModel.BlockCommunitySpam.Should().BeTrue();
        viewModel.BlockPrivateNumbers.Should().BeFalse();
        viewModel.BlockInternational.Should().BeTrue();
        viewModel.BlockedPrefixes.Should().Equal("+57601");
    }

    [Fact]
    public void ChangingAnOption_SavesAllOptions()
    {
        var viewModel = CreateViewModel();

        viewModel.BlockPrivateNumbers = true;

        store.Received(1).Save(Arg.Is<ScreeningSettings>(saved => saved.BlockPrivateNumbers && !saved.BlockCommunitySpam && saved.ShareBlocks));
    }

    [Theory]
    [InlineData("601", "+57601")]
    [InlineData("+1", "+1")]
    [InlineData(" (+44) 20 ", "+4420")]
    public void AddPrefix_NormalizesAndSaves(string typed, string expected)
    {
        var viewModel = CreateViewModel();
        viewModel.NewPrefix = typed;

        viewModel.AddPrefixCommand.Execute(null);

        viewModel.BlockedPrefixes.Should().Equal(expected);
        viewModel.NewPrefix.Should().BeNull();
        store.Received(1).Save(Arg.Is<ScreeningSettings>(saved => saved.BlockedPrefixes.SequenceEqual(new[] { expected })));
    }

    [Theory]
    [InlineData("")]
    [InlineData("3")]
    [InlineData("60a")]
    public void AddPrefix_Invalid_ExplainsAndSavesNothing(string typed)
    {
        var viewModel = CreateViewModel();
        viewModel.NewPrefix = typed;

        viewModel.AddPrefixCommand.Execute(null);

        viewModel.ErrorMessage.Should().Be(Texts.InvalidPrefix);
        store.DidNotReceive().Save(Arg.Any<ScreeningSettings>());
    }

    [Fact]
    public void RemovePrefix_Saves()
    {
        store.Load().Returns(ScreeningSettings.Default with { BlockedPrefixes = ["+57601"] });
        var viewModel = CreateViewModel();

        viewModel.RemovePrefixCommand.Execute("+57601");

        viewModel.BlockedPrefixes.Should().BeEmpty();
        store.Received(1).Save(Arg.Is<ScreeningSettings>(saved => saved.BlockedPrefixes.Count == 0));
    }

    [Fact]
    public async Task StopSharingBlocks_TakesThemBackFromTheServer()
    {
        var blocked = Domain.PhoneNumbers.PhoneNumber.TryParse("3001112233")!;
        blockList.ListAsync().Returns([blocked]);
        var viewModel = CreateViewModel();

        viewModel.ShareBlocks = false;
        await Task.Yield();

        outboxStore.Load().Should().ContainSingle(operation => operation.Kind == PendingOperationKind.Unblock && operation.E164 == blocked.E164);
    }

    [Fact]
    public void SelectTheme_AppliesIt()
    {
        var viewModel = CreateViewModel();

        viewModel.SelectedTheme = viewModel.ThemeOptions.Single(option => option.Theme == AppTheme.Dark);

        themes.Received(1).Apply(AppTheme.Dark);
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

        return new SettingsViewModel(
            store, contribution, permissions, new BlockingService(blockList, store, outbox), themes);
    }
}
