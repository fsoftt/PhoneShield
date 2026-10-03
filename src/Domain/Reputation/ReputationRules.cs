namespace Tranqui.Domain.Reputation;

/// <summary>Tunable thresholds of the reputation algorithm (spec §4.3). Change them here and nowhere else.</summary>
public static class ReputationRules
{
    /// <summary>Votes lose half their weight every period, because phone numbers get reassigned.</summary>
    public static readonly TimeSpan VoteHalfLife = TimeSpan.FromDays(90);

    /// <summary>Minimum decayed spam weight before a number can be flagged.</summary>
    public const double MinimumSpamWeight = 5;

    /// <summary>Spam weight must be this many times larger than the evidence against it.</summary>
    public const double SpamDominanceFactor = 2;

    /// <summary>A name is only shown once this many different people used it (k-anonymity for third-party names).</summary>
    public const int MinimumDistinctContributorsPerName = 3;

    /// <summary>Accounts younger than this vote with reduced weight, which makes fake-account campaigns slower.</summary>
    public static readonly TimeSpan NewAccountPeriod = TimeSpan.FromDays(7);

    public const double NewAccountVoteWeight = 0.5;

    public const double EstablishedAccountVoteWeight = 1;

    public static double ReporterWeight(TimeSpan accountAge) =>
        accountAge < NewAccountPeriod ? NewAccountVoteWeight : EstablishedAccountVoteWeight;

    /// <summary>
    /// Weight of a vote cast <paramref name="age"/> ago, relative to a fresh one. Age counts whole days only, so votes
    /// from the last 24 hours weigh exactly 1 and thresholds are reachable the moment enough people vote.
    /// </summary>
    public static double DecayFactor(TimeSpan age)
    {
        var elapsedDays = Math.Floor(age.TotalDays);

        return elapsedDays <= 0 ? 1 : Math.Pow(0.5, elapsedDays / VoteHalfLife.TotalDays);
    }
}
