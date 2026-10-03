using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Appeals;

/// <summary>A number whose owner asked us to stop showing names for it. Spam reports are unaffected.</summary>
public sealed class HiddenNumber
{
    private HiddenNumber()
    {
        PhoneHash = [];
    }

    private HiddenNumber(PhoneHash phoneHash, DateTimeOffset hiddenAt)
    {
        PhoneHash = phoneHash.Value.ToArray();
        HiddenAt = hiddenAt;
    }

    public byte[] PhoneHash { get; private set; }

    public DateTimeOffset HiddenAt { get; private set; }

    public static HiddenNumber Create(PhoneHash phoneHash, DateTimeOffset hiddenAt)
    {
        ArgumentNullException.ThrowIfNull(phoneHash);

        return new HiddenNumber(phoneHash, hiddenAt);
    }
}
