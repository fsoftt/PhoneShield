namespace Tranqui.Domain.Reputation;

public sealed record SpamVote(ReportVerdict Verdict, double Weight, DateTimeOffset CastAt);

/// <summary>Someone used this name for the number, either as a saved contact or as a spam label.</summary>
public sealed record NameVote(ContributorId Contributor, ProtectedName Name, DateTimeOffset CastAt);

/// <summary>Everything the community contributed about one number.</summary>
public sealed record ReputationSignals(
    IReadOnlyCollection<SpamVote> SpamVotes,
    IReadOnlyCollection<NameVote> NameVotes,
    int SavedByCount)
{
    public static ReputationSignals None { get; } = new([], [], 0);
}
