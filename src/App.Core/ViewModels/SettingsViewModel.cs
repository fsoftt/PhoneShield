using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Contacts;
using Tranqui.App.Core.Protection;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.ViewModels;

/// <summary>
/// Automatic blocking options (saved on change, applied to the next call) and the optional address-book contribution.
/// </summary>
public sealed partial class SettingsViewModel : FormViewModel
{
    private static readonly CompositeFormat contributionStarted = CompositeFormat.Parse(Texts.ContributionStartedFormat);

    private readonly IScreeningSettingsStore store;
    private readonly ContactContributionService contribution;
    private readonly IProtectionPermissions permissions;

    public SettingsViewModel(
        IScreeningSettingsStore store,
        ContactContributionService contribution,
        IProtectionPermissions permissions)
    {
        this.store = store;
        this.contribution = contribution;
        this.permissions = permissions;
        var settings = store.Load();
        BlockCommunitySpam = settings.BlockCommunitySpam;
        BlockPrivateNumbers = settings.BlockPrivateNumbers;
        BlockInternational = settings.BlockInternational;
        IsContributing = contribution.IsContributing;
    }

    [ObservableProperty]
    public partial bool BlockCommunitySpam { get; set; }

    [ObservableProperty]
    public partial bool BlockPrivateNumbers { get; set; }

    [ObservableProperty]
    public partial bool BlockInternational { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotContributing))]
    public partial bool IsContributing { get; set; }

    public bool IsNotContributing => !IsContributing;

    partial void OnBlockCommunitySpamChanged(bool value) => Save();

    partial void OnBlockPrivateNumbersChanged(bool value) => Save();

    partial void OnBlockInternationalChanged(bool value) => Save();

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
            InfoMessage = string.Format(CultureInfo.CurrentCulture, contributionStarted, accepted);
        });
    }

    [RelayCommand]
    private Task StopContributingAsync() => RunAsync(async () =>
    {
        await contribution.StopAsync(CancellationToken.None);
        IsContributing = false;
        InfoMessage = Texts.ContributionStopped;
    });

    private void Save() => store.Save(new ScreeningSettings(BlockCommunitySpam, BlockPrivateNumbers, BlockInternational));
}
