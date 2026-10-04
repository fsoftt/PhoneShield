namespace Tranqui.Domain.Reputation;

/// <summary>Result of evaluating a number. Names are ordered from most to least used and still encrypted.</summary>
public sealed record CallerIdentification(
    CallerStatus Status,
    IReadOnlyList<ProtectedName> RankedNames,
    int SpamReportCount,
    int SavedByCount)
{
    /// <summary>
    /// Spam when the decayed spam weight is high enough and clearly outweighs "not spam" votes plus the trust earned by
    /// being saved in many address books. Otherwise identified when at least one name reached the k-anonymity threshold.
    /// </summary>
    public static CallerIdentification Evaluate(ReputationSignals signals, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(signals);

        var rankedNames = RankNames(signals.NameVotes, now);
        var spamReportCount = signals.SpamVotes.Count(vote => vote.Verdict == ReportVerdict.Spam);
        var status = IsSpam(signals, now) ? CallerStatus.Spam
            : rankedNames.Count > 0 ? CallerStatus.Identified
            : CallerStatus.Unknown;

        return new CallerIdentification(status, rankedNames, spamReportCount, signals.SavedByCount);
    }

    /// <summary>The settled verdict, used to rate reporters: spam, clearly legitimate, or not decided yet.</summary>
    public static CommunityVerdict VerdictOf(ReputationSignals signals, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(signals);

        if (IsSpam(signals, now))
        {
            return CommunityVerdict.Spam;
        }

        return LegitimateEvidence(signals, now) >= ReputationRules.MinimumLegitimateEvidence
            ? CommunityVerdict.Legitimate
            : CommunityVerdict.Undecided;
    }

    private static bool IsSpam(ReputationSignals signals, DateTimeOffset now)
    {
        var spamWeight = SpamWeight(signals, now);

        return spamWeight >= ReputationRules.MinimumSpamWeight
            && spamWeight >= ReputationRules.SpamDominanceFactor * LegitimateEvidence(signals, now);
    }

    /// <summary>Spam reports plus blocks at a reduced factor, with same-day bursts dampened.</summary>
    private static double SpamWeight(ReputationSignals signals, DateTimeOffset now) =>
        DampenedWeight(
            signals.SpamVotes.Where(vote => vote.Verdict == ReportVerdict.Spam).Select(vote => (vote.Weight, vote.CastAt))
                .Concat(signals.BlockVotes.Select(vote => (Weight: vote.Weight * ReputationRules.BlockVoteFactor, vote.CastAt))),
            now);

    private static double LegitimateEvidence(ReputationSignals signals, DateTimeOffset now) =>
        DampenedWeight(
            signals.SpamVotes.Where(vote => vote.Verdict == ReportVerdict.NotSpam).Select(vote => (vote.Weight, vote.CastAt)),
            now)
        + Math.Log2(1 + signals.SavedByCount);

    private static double DampenedWeight(IEnumerable<(double Weight, DateTimeOffset CastAt)> votes, DateTimeOffset now) =>
        votes.GroupBy(vote => vote.CastAt.UtcDateTime.Date)
            .Sum(day => ReputationRules.DampenBurst(day.Sum(vote => vote.Weight * ReputationRules.DecayFactor(now - vote.CastAt))));

    private static List<ProtectedName> RankNames(IEnumerable<NameVote> votes, DateTimeOffset now) =>
        votes.GroupBy(vote => Convert.ToHexString(vote.Name.GroupingKey))
            .Where(group => group.Select(vote => vote.Contributor).Distinct().Count()
                >= ReputationRules.MinimumDistinctContributorsPerName)
            .OrderByDescending(group => group.Sum(vote => ReputationRules.DecayFactor(now - vote.CastAt)))
            .Select(group => group.MaxBy(vote => vote.CastAt)!.Name)
            .ToList();
}
