using Tranqui.Domain.Appeals;

namespace Tranqui.Domain.Tests.Appeals;

public sealed class DeviceIdTests
{
    [Fact]
    public void TryParse_AndroidId_NormalizesToLowercase()
    {
        DeviceId.TryParse("9774D56D682E549C")!.Value.Should().Be("9774d56d682e549c");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("abc-123")]
    [InlineData("ñandú")]
    public void TryParse_InvalidValue_ReturnsNull(string? raw)
    {
        DeviceId.TryParse(raw).Should().BeNull();
    }

    [Fact]
    public void TryParse_TooLong_ReturnsNull()
    {
        DeviceId.TryParse(new string('a', DeviceId.MaxLength + 1)).Should().BeNull();
    }

    [Fact]
    public void ToString_DoesNotRevealTheId()
    {
        DeviceId.TryParse("9774d56d682e549c")!.ToString().Should().NotContain("9774");
    }
}
