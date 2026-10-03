using Android.App.Roles;
using Android.Content.PM;
using AndroidX.Core.Content;
using Android.Content;
using Android.Provider;
using Tranqui.App.Core.Protection;
using AndroidUri = Android.Net.Uri;

namespace Tranqui.App.Calls;

internal sealed class AndroidProtectionPermissions : IProtectionPermissions
{
    private const int CallScreeningRoleRequestCode = 4201;

    private static Context Context => global::Android.App.Application.Context;

    public bool IsCallScreeningEnabled =>
        Context.GetSystemService(Context.RoleService) is RoleManager roles && roles.IsRoleHeld(RoleManager.RoleCallScreening);

    public bool CanShowOverPhoneApp => Settings.CanDrawOverlays(Context);

    public bool CanReadContacts =>
        ContextCompat.CheckSelfPermission(Context, global::Android.Manifest.Permission.ReadContacts) == Permission.Granted;

    public bool CanNotify =>
        Context.GetSystemService(Context.NotificationService) is global::Android.App.NotificationManager manager
        && manager.AreNotificationsEnabled();

    public Task RequestCallScreeningAsync()
    {
        if (Context.GetSystemService(Context.RoleService) is RoleManager roles
            && roles.IsRoleAvailable(RoleManager.RoleCallScreening)
            && Platform.CurrentActivity is { } activity)
        {
            activity.StartActivityForResult(roles.CreateRequestRoleIntent(RoleManager.RoleCallScreening), CallScreeningRoleRequestCode);
        }

        return Task.CompletedTask;
    }

    public Task RequestShowOverPhoneAppAsync()
    {
        var intent = new Intent(Settings.ActionManageOverlayPermission, AndroidUri.Parse($"package:{Context.PackageName}"));
        intent.AddFlags(ActivityFlags.NewTask);
        Context.StartActivity(intent);

        return Task.CompletedTask;
    }

    public Task RequestReadContactsAsync() => Permissions.RequestAsync<Permissions.ContactsRead>();

    public Task RequestNotificationsAsync() => Permissions.RequestAsync<Permissions.PostNotifications>();
}
