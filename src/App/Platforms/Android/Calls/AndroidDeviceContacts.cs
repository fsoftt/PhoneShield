using Android.Content.PM;
using Android.Provider;
using AndroidX.Core.Content;
using Tranqui.App.Core.Calls;
using Tranqui.Domain.PhoneNumbers;
using AndroidUri = Android.Net.Uri;

namespace Tranqui.App.Calls;

/// <summary>Looks the number up in the phone's own address book. Nothing read here is sent anywhere.</summary>
internal sealed class AndroidDeviceContacts : IDeviceContacts
{
    private const string DisplayNameColumn = "display_name";

    public string? FindName(PhoneNumber number)
    {
        var context = global::Android.App.Application.Context;
        if (ContextCompat.CheckSelfPermission(context, global::Android.Manifest.Permission.ReadContacts) != Permission.Granted)
        {
            return null;
        }

        var uri = AndroidUri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, AndroidUri.Encode(number.E164));
        if (uri is null)
        {
            return null;
        }

        using var cursor = context.ContentResolver?.Query(uri, [DisplayNameColumn], null, null, null);

        return cursor is not null && cursor.MoveToFirst() ? cursor.GetString(0) : null;
    }
}
