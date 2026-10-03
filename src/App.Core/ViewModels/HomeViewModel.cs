using CommunityToolkit.Mvvm.ComponentModel;
using Tranqui.App.Core.Protection;
using Tranqui.App.Core.Resources;

namespace Tranqui.App.Core.ViewModels;

/// <summary>Protection status: a checklist of the permissions still missing, refreshed whenever the app resumes.</summary>
public sealed partial class HomeViewModel : ObservableObject
{
    private readonly IProtectionPermissions permissions;
    private readonly ProtectionStep callScreening;
    private readonly ProtectionStep overlay;
    private readonly ProtectionStep contacts;
    private readonly ProtectionStep notifications;

    public HomeViewModel(IProtectionPermissions permissions)
    {
        this.permissions = permissions;
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

    public void Refresh()
    {
        callScreening.IsDone = permissions.IsCallScreeningEnabled;
        overlay.IsDone = permissions.CanShowOverPhoneApp;
        contacts.IsDone = permissions.CanReadContacts;
        notifications.IsDone = permissions.CanNotify;
        IsProtected = Steps.All(step => step.IsDone);
    }
}
