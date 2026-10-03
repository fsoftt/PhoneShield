using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Infrastructure.Reputation;

namespace Tranqui.Infrastructure.Tests.Reputation;

public sealed class AesGcmNameProtectorTests
{
    private static readonly PhoneNumber number = PhoneNumber.TryParse("+573001234567")!;
    private static readonly PhoneNumber otherNumber = PhoneNumber.TryParse("+573009876543")!;
    private static readonly CallerName name = CallerName.TryCreate("Pizzería Juan")!;

    private readonly AesGcmNameProtector protector = CreateProtector();

    [Fact]
    public void Unprotect_ReturnsTheOriginalName()
    {
        var protectedName = protector.Protect(number, name);

        protector.Unprotect(number, protectedName).Should().Be("Pizzería Juan");
    }

    [Fact]
    public void Protect_DoesNotStoreThePlaintext()
    {
        var protectedName = protector.Protect(number, name);

        System.Text.Encoding.UTF8.GetString(protectedName.Ciphertext).Should().NotContain("Juan");
    }

    [Fact]
    public void Protect_SameNameTwice_UsesDifferentCiphertextButSameGroupingKey()
    {
        var first = protector.Protect(number, name);
        var second = protector.Protect(number, CallerName.TryCreate("pizzeria juan")!);

        second.Ciphertext.Should().NotEqual(first.Ciphertext);
        second.GroupingKey.Should().Equal(first.GroupingKey);
    }

    [Fact]
    public void Protect_SameNameForAnotherNumber_HasDifferentGroupingKey()
    {
        protector.Protect(otherNumber, name).GroupingKey.Should().NotEqual(protector.Protect(number, name).GroupingKey);
    }

    [Fact]
    public void Unprotect_WithTheWrongNumber_Fails()
    {
        var protectedName = protector.Protect(number, name);

        var act = () => protector.Unprotect(otherNumber, protectedName);

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Unprotect_TamperedCiphertext_Fails()
    {
        var protectedName = protector.Protect(number, name);
        protectedName.Ciphertext[^1] ^= 0xFF;

        var act = () => protector.Unprotect(number, protectedName);

        act.Should().Throw<CryptographicException>();
    }

    private static AesGcmNameProtector CreateProtector() =>
        new(Options.Create(new NameProtectionOptions { Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)) }));
}
