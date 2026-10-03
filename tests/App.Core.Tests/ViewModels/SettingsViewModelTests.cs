using NSubstitute;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.ViewModels;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class SettingsViewModelTests
{
    private readonly IScreeningSettingsStore store = Substitute.For<IScreeningSettingsStore>();

    [Fact]
    public void Constructor_ShowsTheSavedSettings()
    {
        store.Load().Returns(new ScreeningSettings(BlockCommunitySpam: true, BlockPrivateNumbers: false, BlockInternational: true));

        var viewModel = new SettingsViewModel(store);

        viewModel.BlockCommunitySpam.Should().BeTrue();
        viewModel.BlockPrivateNumbers.Should().BeFalse();
        viewModel.BlockInternational.Should().BeTrue();
    }

    [Fact]
    public void ChangingAnOption_SavesAllOptions()
    {
        store.Load().Returns(ScreeningSettings.Default);
        var viewModel = new SettingsViewModel(store);

        viewModel.BlockPrivateNumbers = true;

        store.Received(1).Save(ScreeningSettings.Default with { BlockPrivateNumbers = true });
    }
}
