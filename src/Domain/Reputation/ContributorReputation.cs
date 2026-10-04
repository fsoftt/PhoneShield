namespace Tranqui.Domain.Reputation;

/// <summary>
/// How much a contributor's votes count, from how often their past reports matched the community's verdict.
/// Recomputed daily by <see cref="ReporterReputation"/>; contributors without a row count at 1.
/// </summary>
public sealed class ContributorReputation
{
    private ContributorReputation()
    {
        ContributorId = [];
    }

    private ContributorReputation(ContributorId contributor, double multiplier, DateTimeOffset computedAt)
    {
        ContributorId = contributor.Value.ToArray();
        Multiplier = multiplier;
        ComputedAt = computedAt;
    }

    public byte[] ContributorId { get; private set; }

    public double Multiplier { get; private set; }

    public DateTimeOffset ComputedAt { get; private set; }

    public static ContributorReputation Create(ContributorId contributor, double multiplier, DateTimeOffset computedAt)
    {
        ArgumentNullException.ThrowIfNull(contributor);

        return new ContributorReputation(contributor, multiplier, computedAt);
    }
}
