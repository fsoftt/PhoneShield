using Tranqui.Domain.Appeals;
using Tranqui.Domain.Reputation;

namespace Tranqui.Domain.Retention;

/// <summary>How long each kind of data is kept before the daily purge deletes it.</summary>
public static class RetentionRules
{
    /// <summary>Quota uses only matter inside the yearly window.</summary>
    public static readonly TimeSpan AppealQuotaUsages = AppealRules.YearWindow;

    /// <summary>Resolved appeals (already without reason or email) are kept a year as a record, then deleted.</summary>
    public static readonly TimeSpan ResolvedAppeals = TimeSpan.FromDays(365);

    /// <summary>
    /// Reports and shared blocks. After eight half-lives a vote weighs under 0.4 % of a fresh one: it no longer changes
    /// any verdict.
    /// </summary>
    public static readonly TimeSpan Votes = ReputationRules.VoteHalfLife * 8;
}
