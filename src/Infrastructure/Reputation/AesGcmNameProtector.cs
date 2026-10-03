using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Infrastructure.Reputation;

/// <summary>
/// Per-number keys: prk = HMAC-SHA256(masterKey, E.164), then HKDF-Expand into an AES-256-GCM key and a grouping key.
/// Without the phone number (and the master key) the stored names cannot be decrypted or even compared.
/// Ciphertext layout: nonce (12) | encrypted name | tag (16).
/// </summary>
internal sealed class AesGcmNameProtector(IOptions<NameProtectionOptions> options) : INameProtector
{
    private const int KeySizeInBytes = 32;
    private static readonly byte[] encryptionKeyInfo = Encoding.UTF8.GetBytes("tranqui/name-encryption/v1");
    private static readonly byte[] groupingKeyInfo = Encoding.UTF8.GetBytes("tranqui/name-grouping/v1");

    private readonly byte[] masterKey = Convert.FromBase64String(options.Value.Key);

    public ProtectedName Protect(PhoneNumber phoneNumber, CallerName name)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        ArgumentNullException.ThrowIfNull(name);

        var (encryptionKey, groupingKey) = DeriveKeys(phoneNumber);
        var plaintext = Encoding.UTF8.GetBytes(name.DisplayValue);
        var output = new byte[AesGcm.NonceByteSizes.MaxSize + plaintext.Length + AesGcm.TagByteSizes.MaxSize];
        var nonce = output.AsSpan(0, AesGcm.NonceByteSizes.MaxSize);
        var ciphertext = output.AsSpan(nonce.Length, plaintext.Length);
        var tag = output.AsSpan(nonce.Length + plaintext.Length);

        RandomNumberGenerator.Fill(nonce);
        using (var aes = new AesGcm(encryptionKey, AesGcm.TagByteSizes.MaxSize))
        {
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
        }

        var grouping = HMACSHA256.HashData(groupingKey, Encoding.UTF8.GetBytes(name.CanonicalValue));

        return new ProtectedName(output, grouping);
    }

    public string Unprotect(PhoneNumber phoneNumber, ProtectedName protectedName)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        ArgumentNullException.ThrowIfNull(protectedName);

        var (encryptionKey, _) = DeriveKeys(phoneNumber);
        var input = protectedName.Ciphertext.AsSpan();
        var nonce = input[..AesGcm.NonceByteSizes.MaxSize];
        var tag = input[^AesGcm.TagByteSizes.MaxSize..];
        var ciphertext = input[nonce.Length..^tag.Length];
        var plaintext = new byte[ciphertext.Length];

        using (var aes = new AesGcm(encryptionKey, AesGcm.TagByteSizes.MaxSize))
        {
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
        }

        return Encoding.UTF8.GetString(plaintext);
    }

    private (byte[] EncryptionKey, byte[] GroupingKey) DeriveKeys(PhoneNumber phoneNumber)
    {
        var pseudoRandomKey = HMACSHA256.HashData(masterKey, Encoding.UTF8.GetBytes(phoneNumber.E164));

        return (
            HKDF.Expand(HashAlgorithmName.SHA256, pseudoRandomKey, KeySizeInBytes, encryptionKeyInfo),
            HKDF.Expand(HashAlgorithmName.SHA256, pseudoRandomKey, KeySizeInBytes, groupingKeyInfo));
    }
}
