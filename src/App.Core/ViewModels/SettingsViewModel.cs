using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Appearance;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Contacts;
using Tranqui.App.Core.Protection;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.ViewModels;

/// <summary>
/// Automatic blocking options and prefixes (saved on change, applied to the next call), sharing blocks as a spam signal,
/// the theme, and the optional address-book contribution.
/// </summary>
public sealed partial class SettingsViewModel : FormViewModel
{
    private readonly IScreeningSettingsStore store;
    private readonly ContactContributionService contribution;
    private readonly IProtectionPermissions permissions;
    private readonly BlockingService blocking;
    private readonly IThemeService themes;
    private bool loaded;

    public SettingsViewModel(
        IScreeningSettingsStore store,
        ContactContributionService contribution,
        IProtectionPermissions permissions,
        BlockingService blocking,
        IThemeService themes)
    {
        this.store = store;
        this.contribution = contribution;
        this.permissions = permissions;
        this.blocking = blocking;
        this.themes = themes;
        var settings = store.Load();
        BlockCommunitySpam = settings.BlockCommunitySpam;
        BlockPrivateNumbers = settings.BlockPrivateNumbers;
        BlockInternational = settings.BlockInternational;
        ShareBlocks = settings.ShareBlocks;
        BlockedPrefixes = new ObservableCollection<string>(settings.BlockedPrefixes);
        IsContributing = contribution.IsContributing;
        SelectedTheme = ThemeOptions.First(option => option.Theme == themes.Current);
        loaded = true;
    }

    public ObservableCollection<string> BlockedPrefixes { get; }

    public IReadOnlyList<ThemeOption> ThemeOptions { get; } = ThemeOption.All;

    [ObservableProperty]
    public partial bool BlockCommunitySpam { get; set; }

    [ObservableProperty]
    public partial bool BlockPrivateNumbers { get; set; }

    [ObservableProperty]
    public partial bool BlockInternational { get; set; }

    [ObservableProperty]
    public partial bool ShareBlocks { get; set; }

    [ObservableProperty]
    public partial string? NewPrefix { get; set; }

    [ObservableProperty]
    public partial ThemeOption SelectedTheme { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotContributing))]
    public partial bool IsContributing { get; set; }

    public bool IsNotContributing => !IsContributing;

    partial void OnBlockCommunitySpamChanged(bool value) => Save();

    partial void OnBlockPrivateNumbersChanged(bool value) => Save();

    partial void OnBlockInternationalChanged(bool value) => Save();

    partial void OnShareBlocksChanged(bool value)
    {
        if (!loaded)
        {
            return;
        }

        Save();
        _ = blocking.SetSharingAsync(value);
    }

    partial void OnSelectedThemeChanged(ThemeOption value)
    {
        if (loaded && value is not null)
        {
            themes.Apply(value.Theme);
        }
    }

    [RelayCommand]
    private void AddPrefix()
    {
        var prefix = BlockedPrefix.TryNormalize(NewPrefix);
        if (!Require(prefix is not null, Texts.InvalidPrefix))
        {
            return;
        }

        ErrorMessage = null;
        if (!BlockedPrefixes.Contains(prefix!))
        {
            BlockedPrefixes.Add(prefix!);
            Save();
        }

        NewPrefix = null;
    }

    [RelayCommand]
    private void RemovePrefix(string prefix)
    {
        if (BlockedPrefixes.Remove(prefix))
        {
            Save();
        }
    }

    [RelayCommand]
    private async Task StartContributingAsync()
    {
        if (!permissions.CanReadContacts)
        {
            await permissions.RequestReadContactsAsync();
        }

        if (!Require(permissions.CanReadContacts, Texts.ContactsPermissionNeeded))
        {
            return;
        }

        await RunAsync(async () =>
        {
            var accepted = await contribution.StartAsync(CancellationToken.None);
            IsContributing = true;
            InfoMessage = Texts.Format(Texts.ContributionStartedFormat, accepted);
        });
    }

    [RelayCommand]
    private Task StopContributingAsync() => RunAsync(async () =>
    {
        await contribution.StopAsync(CancellationToken.None);
        IsContributing = false;
        InfoMessage = Texts.ContributionStopped;
    });

    private void Save()
    {
        if (loaded)
        {
            store.Save(new ScreeningSettings(
                BlockCommunitySpam, BlockPrivateNumbers, BlockInternational, [.. BlockedPrefixes], ShareBlocks));
        }
    }
}
