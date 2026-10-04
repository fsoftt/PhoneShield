using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Reputation;

/// <summary>Someone blocked this number on their phone: a weak spam signal (see <see cref="ReputationRules.BlockVoteFactor"/>).</summary>
public sealed class BlockSignal
{
    private BlockSignal()
    {
        PhoneHash = [];
        ContributorId = [];
    }

    private BlockSignal(PhoneHash phoneHash, ContributorId contributor, double weight, DateTimeOffset blockedAt)
    {
        PhoneHash = phoneHash.Value.ToArray();
        ContributorId = contributor.Value.ToArray();
        Weight = weight;
        BlockedAt = blockedAt;
    }

    public byte[] PhoneHash { get; private set; }

    public byte[] ContributorId { get; private set; }

    public double Weight { get; private set; }

    public DateTimeOffset BlockedAt { get; private set; }

    public static BlockSignal Create(PhoneHash phoneHash, ContributorId contributor, double weight, DateTimeOffset blockedAt)
    {
        ArgumentNullException.ThrowIfNull(phoneHash);
        ArgumentNullException.ThrowIfNull(contributor);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weight);

        return new BlockSignal(phoneHash, contributor, weight, blockedAt);
    }
}
