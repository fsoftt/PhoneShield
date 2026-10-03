using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Authentication;
using Tranqui.App.Core.Dialogs;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.ViewModels;

/// <summary>See what the server keeps about you, sign out, or delete everything (server data first, then the Firebase identity).</summary>
public sealed partial class AccountViewModel(
    ITranquiApi api,
    IAuthService authService,
    IDialogService dialogs,
    INavigationService navigation) : FormViewModel
{
    public string? Email => authService.Email;

    [ObservableProperty]
    public partial string? MyDataSummary { get; set; }

    [RelayCommand]
    private Task ShowMyDataAsync() => RunAsync(async () =>
    {
        var data = await api.ExportMyDataAsync(CancellationToken.None);
        MyDataSummary = Texts.Format(
            Texts.MyDataSummaryFormat, data.CreatedAt.LocalDateTime, data.SpamReportCount, data.ContactContributionCount);
    });

    [RelayCommand]
    private Task SignOutAsync()
    {
        authService.SignOut();
        return navigation.GoToAsync(Routes.SignIn);
    }

    [RelayCommand]
    private async Task DeleteAccountAsync()
    {
        if (!await dialogs.ConfirmAsync(Texts.DeleteAccountTitle, Texts.DeleteAccountMessage, Texts.DeleteAccountAccept, Texts.Cancel))
        {
            return;
        }

        await RunAsync(async () =>
        {
            await api.DeleteAccountAsync(CancellationToken.None);
            await authService.DeleteIdentityAsync(CancellationToken.None);
            await navigation.GoToAsync(Routes.SignIn);
        });
    }
}
