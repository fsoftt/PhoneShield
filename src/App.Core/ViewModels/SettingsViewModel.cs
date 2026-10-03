using CommunityToolkit.Mvvm.ComponentModel;
using Tranqui.App.Core.Calls;

namespace Tranqui.App.Core.ViewModels;

/// <summary>Automatic blocking options. Each change is saved immediately and applies to the next call.</summary>
public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IScreeningSettingsStore store;

    public SettingsViewModel(IScreeningSettingsStore store)
    {
        this.store = store;
        var settings = store.Load();
        BlockCommunitySpam = settings.BlockCommunitySpam;
        BlockPrivateNumbers = settings.BlockPrivateNumbers;
        BlockInternational = settings.BlockInternational;
    }

    [ObservableProperty]
    public partial bool BlockCommunitySpam { get; set; }

    [ObservableProperty]
    public partial bool BlockPrivateNumbers { get; set; }

    [ObservableProperty]
    public partial bool BlockInternational { get; set; }

    partial void OnBlockCommunitySpamChanged(bool value) => Save();

    partial void OnBlockPrivateNumbersChanged(bool value) => Save();

    partial void OnBlockInternationalChanged(bool value) => Save();

    private void Save() => store.Save(new ScreeningSettings(BlockCommunitySpam, BlockPrivateNumbers, BlockInternational));
}
