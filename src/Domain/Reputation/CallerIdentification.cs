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

    private static bool IsSpam(ReputationSignals signals, DateTimeOffset now)
    {
        var spamWeight = DecayedWeight(signals.SpamVotes, ReportVerdict.Spam, now);
        var notSpamWeight = DecayedWeight(signals.SpamVotes, ReportVerdict.NotSpam, now);
        var trust = Math.Log2(1 + signals.SavedByCount);

        return spamWeight >= ReputationRules.MinimumSpamWeight
            && spamWeight >= ReputationRules.SpamDominanceFactor * (notSpamWeight + trust);
    }

    private static double DecayedWeight(IEnumerable<SpamVote> votes, ReportVerdict verdict, DateTimeOffset now) =>
        votes.Where(vote => vote.Verdict == verdict)
            .Sum(vote => vote.Weight * ReputationRules.DecayFactor(now - vote.CastAt));

    private static List<ProtectedName> RankNames(IEnumerable<NameVote> votes, DateTimeOffset now) =>
        votes.GroupBy(vote => Convert.ToHexString(vote.Name.GroupingKey))
            .Where(group => group.Select(vote => vote.Contributor).Distinct().Count()
                >= ReputationRules.MinimumDistinctContributorsPerName)
            .OrderByDescending(group => group.Sum(vote => ReputationRules.DecayFactor(now - vote.CastAt)))
            .Select(group => group.MaxBy(vote => vote.CastAt)!.Name)
            .ToList();
}
