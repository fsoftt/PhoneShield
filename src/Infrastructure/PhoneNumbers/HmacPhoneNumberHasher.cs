using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Infrastructure.PhoneNumbers;

/// <summary>
/// HMAC-SHA256 of the E.164 number with key version 1, then re-keyed with each later version in order:
/// hash(v) = HMAC(key[v], hash(v - 1)). This lets keys rotate without ever knowing the original numbers.
/// </summary>
internal sealed class HmacPhoneNumberHasher : IPhoneNumberHasher
{
    private readonly byte[][] keysInVersionOrder;
    private readonly int currentKeyVersion;

    public HmacPhoneNumberHasher(IOptions<PhoneHashingOptions> options)
    {
        currentKeyVersion = options.Value.CurrentKeyVersion;
        keysInVersionOrder = Enumerable.Range(1, currentKeyVersion)
            .Select(version => Convert.FromBase64String(options.Value.Keys[version]))
            .ToArray();
    }

    public PhoneHash Hash(PhoneNumber phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);

        var hash = HMACSHA256.HashData(keysInVersionOrder[0], Encoding.UTF8.GetBytes(phoneNumber.E164));
        foreach (var key in keysInVersionOrder.Skip(1))
        {
            hash = HMACSHA256.HashData(key, hash);
        }

        return new PhoneHash(hash, currentKeyVersion);
    }
}
