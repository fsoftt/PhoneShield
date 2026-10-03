using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Appeals;

/// <summary>Record that a verification SMS was requested for a number (stored hashed), used to cap SMS per number.</summary>
public sealed class SmsVerificationRequest
{
    private SmsVerificationRequest()
    {
        PhoneHash = [];
    }

    private SmsVerificationRequest(PhoneHash phoneHash, DateTimeOffset requestedAt)
    {
        Id = Guid.CreateVersion7(requestedAt);
        PhoneHash = phoneHash.Value.ToArray();
        RequestedAt = requestedAt;
    }

    public Guid Id { get; private set; }

    public byte[] PhoneHash { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public static SmsVerificationRequest Create(PhoneHash phoneHash, DateTimeOffset requestedAt)
    {
        ArgumentNullException.ThrowIfNull(phoneHash);

        return new SmsVerificationRequest(phoneHash, requestedAt);
    }
}
