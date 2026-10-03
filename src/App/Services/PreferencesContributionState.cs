using Tranqui.App.Core.Contacts;

namespace Tranqui.App.Services;

internal sealed class PreferencesContributionState : IContributionState
{
    private const string IsContributingKey = "tranqui.contributing";
    private const string LastSyncedAtKey = "tranqui.contacts_last_synced_at";

    public bool IsContributing
    {
        get => Preferences.Default.Get(IsContributingKey, false);
        set => Preferences.Default.Set(IsContributingKey, value);
    }

    public DateTimeOffset? LastSyncedAt
    {
        get => Preferences.Default.ContainsKey(LastSyncedAtKey)
            ? DateTimeOffset.FromUnixTimeSeconds(Preferences.Default.Get(LastSyncedAtKey, 0L))
            : null;
        set
        {
            if (value is null)
            {
                Preferences.Default.Remove(LastSyncedAtKey);
            }
            else
            {
                Preferences.Default.Set(LastSyncedAtKey, value.Value.ToUnixTimeSeconds());
            }
        }
    }
}
