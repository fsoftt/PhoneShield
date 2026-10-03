using Tranqui.Application.Features.LookupNumber;

namespace Tranqui.Application.Tests.Features.LookupNumber;

public sealed class LookupNumberValidatorTests
{
    private readonly LookupNumberValidator validator = new();

    [Fact]
    public void Validate_ValidNumber_IsValid()
    {
        validator.Validate(new LookupNumberQuery("3001234567")).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    public void Validate_InvalidNumber_IsInvalid(string raw)
    {
        validator.Validate(new LookupNumberQuery(raw)).IsValid.Should().BeFalse();
    }
}
