using NSubstitute;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.Onboarding;
using Tranqui.App.Core.Protection;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class HomeViewModelTests
{
    private readonly IProtectionPermissions permissions = Substitute.For<IProtectionPermissions>();
    private readonly IOnboardingState onboarding = Substitute.For<IOnboardingState>();
    private readonly INavigationService navigation = Substitute.For<INavigationService>();

    [Fact]
    public void Refresh_MissingPermissions_ListsThemAsPending()
    {
        permissions.IsCallScreeningEnabled.Returns(true);

        var viewModel = new HomeViewModel(permissions, onboarding, navigation);

        viewModel.IsProtected.Should().BeFalse();
        viewModel.StatusTitle.Should().Be(Texts.ProtectionIncomplete);
        viewModel.Steps.Count(step => step.IsPending).Should().Be(3);
    }

    [Fact]
    public void Refresh_AllPermissionsGranted_IsProtected()
    {
        var viewModel = new HomeViewModel(permissions, onboarding, navigation);
        permissions.IsCallScreeningEnabled.Returns(true);
        permissions.CanShowOverPhoneApp.Returns(true);
        permissions.CanReadContacts.Returns(true);
        permissions.CanNotify.Returns(true);

        viewModel.Refresh();

        viewModel.IsProtected.Should().BeTrue();
        viewModel.StatusTitle.Should().Be(Texts.ProtectionActive);
    }

    [Fact]
    public async Task EnableStep_RequestsItsPermission()
    {
        var viewModel = new HomeViewModel(permissions, onboarding, navigation);

        await viewModel.Steps[0].EnableCommand.ExecuteAsync(null);

        await permissions.Received(1).RequestCallScreeningAsync();
    }

    [Fact]
    public void FirstOpen_ExplainsThatBlocksAreShared()
    {
        new HomeViewModel(permissions, onboarding, navigation).ShowBlockSharingNotice.Should().BeTrue();
    }

    [Fact]
    public void NoticeAlreadySeen_IsNotShownAgain()
    {
        onboarding.BlockSharingNoticeSeen.Returns(true);

        new HomeViewModel(permissions, onboarding, navigation).ShowBlockSharingNotice.Should().BeFalse();
    }

    [Fact]
    public void DismissNotice_RemembersIt()
    {
        var viewModel = new HomeViewModel(permissions, onboarding, navigation);

        viewModel.DismissBlockSharingNoticeCommand.Execute(null);

        viewModel.ShowBlockSharingNotice.Should().BeFalse();
        onboarding.Received().BlockSharingNoticeSeen = true;
    }

    [Fact]
    public async Task ChangeInSettings_OpensSettingsAndDismisses()
    {
        var viewModel = new HomeViewModel(permissions, onboarding, navigation);

        await viewModel.OpenSettingsCommand.ExecuteAsync(null);

        await navigation.Received(1).GoToAsync(Routes.Settings);
        viewModel.ShowBlockSharingNotice.Should().BeFalse();
    }
}
