using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Infrastructure.PhoneNumbers;

namespace Tranqui.Infrastructure.Tests.PhoneNumbers;

public sealed class HmacPhoneNumberHasherTests
{
    private const string E164 = "+573001234567";

    private static readonly byte[] firstKey = RandomNumberGenerator.GetBytes(32);
    private static readonly byte[] secondKey = RandomNumberGenerator.GetBytes(32);
    private static readonly PhoneNumber phoneNumber = PhoneNumber.TryParse(E164)!;

    [Fact]
    public void Hash_SingleKey_IsHmacSha256OfE164()
    {
        var hasher = CreateHasher(firstKey);

        var hash = hasher.Hash(phoneNumber);

        hash.Value.ToArray().Should().Equal(HMACSHA256.HashData(firstKey, Encoding.UTF8.GetBytes(E164)));
        hash.KeyVersion.Should().Be(1);
    }

    [Fact]
    public void Hash_SameNumberInDifferentFormats_ProducesSameHash()
    {
        var hasher = CreateHasher(firstKey);

        hasher.Hash(PhoneNumber.TryParse("300 123 4567")!).Should().Be(hasher.Hash(phoneNumber));
    }

    [Fact]
    public void Hash_DifferentKeys_ProduceDifferentHashes()
    {
        CreateHasher(firstKey).Hash(phoneNumber).Should().NotBe(CreateHasher(secondKey).Hash(phoneNumber));
    }

    [Fact]
    public void Hash_AfterRotation_ChainsNewKeyOverPreviousHash()
    {
        var previousHash = CreateHasher(firstKey).Hash(phoneNumber);
        var rotatedHasher = CreateHasher(firstKey, secondKey);

        var hash = rotatedHasher.Hash(phoneNumber);

        hash.KeyVersion.Should().Be(2);
        hash.Value.ToArray().Should().Equal(HMACSHA256.HashData(secondKey, previousHash.Value));
    }

    private static HmacPhoneNumberHasher CreateHasher(params byte[][] keys)
    {
        var options = new PhoneHashingOptions
        {
            CurrentKeyVersion = keys.Length,
            Keys = keys.Select((key, index) => (Version: index + 1, Key: Convert.ToBase64String(key)))
                .ToDictionary(entry => entry.Version, entry => entry.Key),
        };

        return new HmacPhoneNumberHasher(Options.Create(options));
    }
}
