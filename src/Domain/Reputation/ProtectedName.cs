namespace Tranqui.Domain.Reputation;

/// <summary>
/// A caller name encrypted with a key derived from its phone number, plus a keyed grouping key that lets identical
/// names be counted together without decrypting them. Both are opaque to the domain.
/// </summary>
public sealed record ProtectedName(byte[] Ciphertext, byte[] GroupingKey);
