using Android.Content.PM;
using Android.Provider;
using AndroidX.Core.Content;
using Tranqui.App.Core.Contacts;

namespace Tranqui.App.Calls;

/// <summary>Every phone number in the address book with its display name. Read only when the user chose to contribute.</summary>
internal sealed class AndroidDeviceContactSource : IDeviceContactSource
{
    private const string NumberColumn = "data1";
    private const string DisplayNameColumn = "display_name";

    public IReadOnlyList<DeviceContact> ReadAll()
    {
        var context = global::Android.App.Application.Context;
        if (ContextCompat.CheckSelfPermission(context, global::Android.Manifest.Permission.ReadContacts) != Permission.Granted
            || ContactsContract.CommonDataKinds.Phone.ContentUri is not { } uri)
        {
            return [];
        }

        var contacts = new List<DeviceContact>();
        using var cursor = context.ContentResolver?.Query(uri, [NumberColumn, DisplayNameColumn], null, null, null);
        while (cursor is not null && cursor.MoveToNext())
        {
            var number = cursor.GetString(0);
            if (!string.IsNullOrWhiteSpace(number))
            {
                contacts.Add(new DeviceContact(number, cursor.GetString(1)));
            }
        }

        return contacts;
    }
}
