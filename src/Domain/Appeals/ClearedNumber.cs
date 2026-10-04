using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Appeals;

/// <summary>
/// A number whose spam review was approved: spam reports cast before <see cref="ClearedAt"/> no longer count.
/// Reports made afterwards count again, so a number that goes back to spamming is flagged again.
/// </summary>
public sealed class ClearedNumber
{
    private ClearedNumber()
    {
        PhoneHash = [];
    }

    private ClearedNumber(PhoneHash phoneHash, DateTimeOffset clearedAt)
    {
        PhoneHash = phoneHash.Value.ToArray();
        ClearedAt = clearedAt;
    }

    public byte[] PhoneHash { get; private set; }

    public DateTimeOffset ClearedAt { get; private set; }

    public static ClearedNumber Create(PhoneHash phoneHash, DateTimeOffset clearedAt)
    {
        ArgumentNullException.ThrowIfNull(phoneHash);

        return new ClearedNumber(phoneHash, clearedAt);
    }

    public void ClearAgain(DateTimeOffset clearedAt) => ClearedAt = clearedAt;
}
