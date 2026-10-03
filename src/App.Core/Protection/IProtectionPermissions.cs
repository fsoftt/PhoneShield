namespace Tranqui.App.Core.Protection;

/// <summary>The Android permissions the protection needs. Requests open system screens; re-check when the app resumes.</summary>
public interface IProtectionPermissions
{
    bool IsCallScreeningEnabled { get; }

    bool CanShowOverPhoneApp { get; }

    bool CanReadContacts { get; }

    bool CanNotify { get; }

    Task RequestCallScreeningAsync();

    Task RequestShowOverPhoneAppAsync();

    Task RequestReadContactsAsync();

    Task RequestNotificationsAsync();
}
