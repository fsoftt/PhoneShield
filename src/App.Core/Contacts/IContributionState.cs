namespace Tranqui.App.Core.Contacts;

/// <summary>Whether this device contributes its address book, and when it last synced.</summary>
public interface IContributionState
{
    bool IsContributing { get; set; }

    DateTimeOffset? LastSyncedAt { get; set; }
}
