using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Reputation;

/// <summary>One person's verdict on a number after a call, optionally with a label such as "Spam Claro".</summary>
public sealed class SpamReport
{
    private SpamReport()
    {
        PhoneHash = [];
        ContributorId = [];
    }

    private SpamReport(
        PhoneHash phoneHash,
        ContributorId contributor,
        ReportVerdict verdict,
        double weight,
        ProtectedName? label,
        DateTimeOffset reportedAt)
    {
        Id = Guid.CreateVersion7(reportedAt);
        PhoneHash = phoneHash.Value.ToArray();
        HashKeyVersion = phoneHash.KeyVersion;
        ContributorId = contributor.Value.ToArray();
        Verdict = verdict;
        Weight = weight;
        LabelCiphertext = label?.Ciphertext;
        LabelGroupingKey = label?.GroupingKey;
        ReportedAt = reportedAt;
    }

    public Guid Id { get; private set; }

    public byte[] PhoneHash { get; private set; }

    public int HashKeyVersion { get; private set; }

    public byte[] ContributorId { get; private set; }

    public ReportVerdict Verdict { get; private set; }

    public double Weight { get; private set; }

    public byte[]? LabelCiphertext { get; private set; }

    public byte[]? LabelGroupingKey { get; private set; }

    public DateTimeOffset ReportedAt { get; private set; }

    public ProtectedName? Label =>
        LabelCiphertext is null || LabelGroupingKey is null ? null : new ProtectedName(LabelCiphertext, LabelGroupingKey);

    public static SpamReport Create(
        PhoneHash phoneHash,
        ContributorId contributor,
        ReportVerdict verdict,
        double weight,
        ProtectedName? label,
        DateTimeOffset reportedAt)
    {
        ArgumentNullException.ThrowIfNull(phoneHash);
        ArgumentNullException.ThrowIfNull(contributor);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weight);

        if (verdict == ReportVerdict.NotSpam && label is not null)
        {
            throw new ArgumentException("Only spam reports can carry a label.", nameof(label));
        }

        return new SpamReport(phoneHash, contributor, verdict, weight, label, reportedAt);
    }
}
