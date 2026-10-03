namespace Tranqui.Domain.Reputation;

/// <summary>
/// Pseudonymous identifier of a contributing user (keyed hash of the Firebase uid). Lets a user's contributions be
/// counted once and withdrawn later without storing which account made them.
/// </summary>
public sealed class ContributorId : IEquatable<ContributorId>
{
    public const int SizeInBytes = 32;

    private readonly byte[] value;

    public ContributorId(ReadOnlySpan<byte> value)
    {
        if (value.Length != SizeInBytes)
        {
            throw new ArgumentException($"A contributor id must be exactly {SizeInBytes} bytes.", nameof(value));
        }

        this.value = value.ToArray();
    }

    public ReadOnlySpan<byte> Value => value;

    public bool Equals(ContributorId? other) => other is not null && value.AsSpan().SequenceEqual(other.value);

    public override bool Equals(object? obj) => Equals(obj as ContributorId);

    public override int GetHashCode() => BitConverter.ToInt32(value, 0);

    public override string ToString() => Convert.ToHexStringLower(value);
}
