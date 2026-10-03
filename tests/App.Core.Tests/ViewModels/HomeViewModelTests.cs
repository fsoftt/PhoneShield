using NSubstitute;
using Tranqui.App.Core.Protection;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class HomeViewModelTests
{
    private readonly IProtectionPermissions permissions = Substitute.For<IProtectionPermissions>();

    [Fact]
    public void Refresh_MissingPermissions_ListsThemAsPending()
    {
        permissions.IsCallScreeningEnabled.Returns(true);

        var viewModel = new HomeViewModel(permissions);

        viewModel.IsProtected.Should().BeFalse();
        viewModel.StatusTitle.Should().Be(Texts.ProtectionIncomplete);
        viewModel.Steps.Count(step => step.IsPending).Should().Be(3);
    }

    [Fact]
    public void Refresh_AllPermissionsGranted_IsProtected()
    {
        var viewModel = new HomeViewModel(permissions);
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
        var viewModel = new HomeViewModel(permissions);

        await viewModel.Steps[0].EnableCommand.ExecuteAsync(null);

        await permissions.Received(1).RequestCallScreeningAsync();
    }
}
