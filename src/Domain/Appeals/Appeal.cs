using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Appeals;

/// <summary>
/// A request from the verified owner of a number. The reason and the optional contact email exist only to handle a
/// spam review (legal deadlines apply) and are erased as soon as it is resolved.
/// </summary>
public sealed class Appeal
{
    private Appeal()
    {
        PhoneHash = [];
    }

    private Appeal(PhoneHash phoneHash, AppealKind kind, string? reason, string? contactEmail, DateTimeOffset createdAt)
    {
        Id = Guid.CreateVersion7(createdAt);
        PhoneHash = phoneHash.Value.ToArray();
        HashKeyVersion = phoneHash.KeyVersion;
        Kind = kind;
        Reason = reason;
        ContactEmail = contactEmail;
        CreatedAt = createdAt;
        Status = kind == AppealKind.HideNames ? AppealStatus.Applied : AppealStatus.Pending;
    }

    public Guid Id { get; private set; }

    public byte[] PhoneHash { get; private set; }

    public int HashKeyVersion { get; private set; }

    public AppealKind Kind { get; private set; }

    public string? Reason { get; private set; }

    public string? ContactEmail { get; private set; }

    public AppealStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ResolvedAt { get; private set; }

    /// <summary>When a pending review must be answered by: Colombian law gives 15 business days for complaints.</summary>
    public DateTimeOffset DueAt => BusinessDays.Add(CreatedAt, AppealRules.ReviewDeadlineBusinessDays);

    public PhoneHash Hash => new(PhoneHash, HashKeyVersion);

    /// <summary>A person decided a spam review. The reason and email are erased: they were only needed until now.</summary>
    public void Resolve(AppealDecision decision, DateTimeOffset now)
    {
        if (Status != AppealStatus.Pending)
        {
            throw new InvalidOperationException("Only pending appeals can be resolved.");
        }

        Status = decision switch
        {
            AppealDecision.Approve => AppealStatus.Approved,
            AppealDecision.Reject => AppealStatus.Rejected,
            _ => throw new ArgumentOutOfRangeException(nameof(decision)),
        };
        ResolvedAt = now;
        Reason = null;
        ContactEmail = null;
    }

    public static Appeal File(PhoneHash phoneHash, AppealKind kind, string? reason, string? contactEmail, DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(phoneHash);

        if (reason?.Length > AppealRules.ReasonMaxLength)
        {
            throw new ArgumentException($"The reason can have at most {AppealRules.ReasonMaxLength} characters.", nameof(reason));
        }

        return new Appeal(phoneHash, kind, string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim(), createdAt);
    }
}
