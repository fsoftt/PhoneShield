using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Navigation;
using Tranqui.App.Core.Onboarding;
using Tranqui.App.Core.Protection;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.ViewModels;

/// <summary>
/// Protection status: a checklist of the permissions still missing, refreshed whenever the app resumes. The first time,
/// it also explains that blocks are shared as a spam signal (on by default) and where to turn that off.
/// </summary>
public sealed partial class HomeViewModel : ObservableObject
{
    private readonly IProtectionPermissions permissions;
    private readonly IOnboardingState onboarding;
    private readonly INavigationService navigation;
    private readonly ProtectionStep callScreening;
    private readonly ProtectionStep overlay;
    private readonly ProtectionStep contacts;
    private readonly ProtectionStep notifications;

    public HomeViewModel(IProtectionPermissions permissions, IOnboardingState onboarding, INavigationService navigation)
    {
        this.permissions = permissions;
        this.onboarding = onboarding;
        this.navigation = navigation;
        ShowBlockSharingNotice = !onboarding.BlockSharingNoticeSeen;
        callScreening = new(Texts.StepCallScreeningTitle, Texts.StepCallScreeningExplanation, permissions.RequestCallScreeningAsync);
        overlay = new(Texts.StepOverlayTitle, Texts.StepOverlayExplanation, permissions.RequestShowOverPhoneAppAsync);
        contacts = new(Texts.StepContactsTitle, Texts.StepContactsExplanation, permissions.RequestReadContactsAsync);
        notifications = new(Texts.StepNotificationsTitle, Texts.StepNotificationsExplanation, permissions.RequestNotificationsAsync);
        Steps = [callScreening, overlay, contacts, notifications];
        Refresh();
    }

    public IReadOnlyList<ProtectionStep> Steps { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusTitle))]
    public partial bool IsProtected { get; set; }

    public string StatusTitle => IsProtected ? Texts.ProtectionActive : Texts.ProtectionIncomplete;

    [ObservableProperty]
    public partial bool ShowBlockSharingNotice { get; set; }

    public void Refresh()
    {
        callScreening.IsDone = permissions.IsCallScreeningEnabled;
        overlay.IsDone = permissions.CanShowOverPhoneApp;
        contacts.IsDone = permissions.CanReadContacts;
        notifications.IsDone = permissions.CanNotify;
        IsProtected = Steps.All(step => step.IsDone);
    }

    [RelayCommand]
    private void DismissBlockSharingNotice()
    {
        onboarding.BlockSharingNoticeSeen = true;
        ShowBlockSharingNotice = false;
    }

    [RelayCommand]
    private Task OpenSettingsAsync()
    {
        DismissBlockSharingNotice();
        return navigation.GoToAsync(Routes.Settings);
    }
}
