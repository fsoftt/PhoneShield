namespace Tranqui.Domain.PhoneNumbers;

/// <summary>
/// Keyed hash of a normalized phone number: the only form in which a number is ever persisted server-side.
/// <see cref="KeyVersion"/> records which secret key generation produced it, so keys can be rotated.
/// </summary>
public sealed class PhoneHash : IEquatable<PhoneHash>
{
    public const int SizeInBytes = 32;

    private readonly byte[] value;

    public PhoneHash(ReadOnlySpan<byte> value, int keyVersion)
    {
        if (value.Length != SizeInBytes)
        {
            throw new ArgumentException($"A phone hash must be exactly {SizeInBytes} bytes.", nameof(value));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(keyVersion, 1);

        this.value = value.ToArray();
        KeyVersion = keyVersion;
    }

    public ReadOnlySpan<byte> Value => value;

    public int KeyVersion { get; }

    public string ToHex() => Convert.ToHexStringLower(value);

    public bool Equals(PhoneHash? other) =>
        other is not null && KeyVersion == other.KeyVersion && value.AsSpan().SequenceEqual(other.value);

    public override bool Equals(object? obj) => Equals(obj as PhoneHash);

    public override int GetHashCode() => HashCode.Combine(KeyVersion, BitConverter.ToInt32(value, 0));

    public override string ToString() => ToHex();
}
