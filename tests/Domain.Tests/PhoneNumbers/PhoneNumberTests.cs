using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Tests.PhoneNumbers;

public sealed class PhoneNumberTests
{
    private const string ColombianMobileE164 = "+573001234567";

    [Theory]
    [InlineData("3001234567")]
    [InlineData("300 123 4567")]
    [InlineData("(300) 123-4567")]
    [InlineData("+57 300 123 4567")]
    [InlineData("+573001234567")]
    public void TryParse_ColombianMobileInAnyFormat_NormalizesToSameE164(string raw)
    {
        var phoneNumber = PhoneNumber.TryParse(raw);

        phoneNumber.Should().NotBeNull();
        phoneNumber!.E164.Should().Be(ColombianMobileE164);
        phoneNumber.CountryCode.Should().Be(57);
    }

    [Fact]
    public void TryParse_BogotaLandline_NormalizesToE164()
    {
        var phoneNumber = PhoneNumber.TryParse("601 234 5678");

        phoneNumber!.E164.Should().Be("+576012345678");
    }

    [Fact]
    public void TryParse_ForeignNumberWithCountryCode_KeepsItsCountry()
    {
        var phoneNumber = PhoneNumber.TryParse("+1 650 253 0000");

        phoneNumber!.E164.Should().Be("+16502530000");
        phoneNumber.CountryCode.Should().Be(1);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("not a number")]
    [InlineData("+57 999 999 9999")]
    public void TryParse_InvalidInput_ReturnsNull(string? raw)
    {
        PhoneNumber.TryParse(raw).Should().BeNull();
    }

    [Fact]
    public void TryParse_SameNumberDifferentFormats_AreEqual()
    {
        PhoneNumber.TryParse("300 123 4567").Should().Be(PhoneNumber.TryParse("+573001234567"));
    }

    [Fact]
    public void ToString_NeverExposesTheFullNumber()
    {
        var phoneNumber = PhoneNumber.TryParse(ColombianMobileE164)!;

        phoneNumber.ToString().Should().Be("+57 •••••••567");
        $"{phoneNumber}".Should().NotContain("3001234");
    }
}
