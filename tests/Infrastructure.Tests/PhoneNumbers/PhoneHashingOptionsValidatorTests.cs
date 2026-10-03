using System.Security.Cryptography;
using Tranqui.Infrastructure.PhoneNumbers;

namespace Tranqui.Infrastructure.Tests.PhoneNumbers;

public sealed class PhoneHashingOptionsValidatorTests
{
    private static readonly string validKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private readonly PhoneHashingOptionsValidator validator = new();

    [Fact]
    public void Validate_AllVersionsConfigured_Succeeds()
    {
        var options = new PhoneHashingOptions { CurrentKeyVersion = 2, Keys = { [1] = validKey, [2] = validKey } };

        validator.Validate(null, options).Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_NoCurrentVersion_Fails()
    {
        validator.Validate(null, new PhoneHashingOptions()).Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_MissingEarlierVersion_Fails()
    {
        var options = new PhoneHashingOptions { CurrentKeyVersion = 2, Keys = { [2] = validKey } };

        validator.Validate(null, options).Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_KeyTooShort_Fails()
    {
        var shortKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        var options = new PhoneHashingOptions { CurrentKeyVersion = 1, Keys = { [1] = shortKey } };

        validator.Validate(null, options).Failed.Should().BeTrue();
    }

    [Fact]
    public void Validate_KeyNotBase64_FailsWithoutEchoingTheKey()
    {
        const string invalidKey = "this-is-not-base64-but-is-long-enough-to-matter!!";
        var options = new PhoneHashingOptions { CurrentKeyVersion = 1, Keys = { [1] = invalidKey } };

        var result = validator.Validate(null, options);

        result.Failed.Should().BeTrue();
        result.FailureMessage.Should().NotContain(invalidKey);
    }
}
