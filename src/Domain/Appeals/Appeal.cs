using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Appeals;

/// <summary>
/// A request from the verified owner of a number. The optional contact email exists only to answer a spam review
/// (legal deadlines apply) and is deleted with the appeal once it is resolved.
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
        Kind = kind;
        Reason = reason;
        ContactEmail = contactEmail;
        CreatedAt = createdAt;
        Status = kind == AppealKind.HideNames ? AppealStatus.Applied : AppealStatus.Pending;
    }

    public Guid Id { get; private set; }

    public byte[] PhoneHash { get; private set; }

    public AppealKind Kind { get; private set; }

    public string? Reason { get; private set; }

    public string? ContactEmail { get; private set; }

    public AppealStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

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
