using Tranqui.Application.Features.RegisterAccount;
using Tranqui.Domain.Legal;

namespace Tranqui.Application.Tests.Features.RegisterAccount;

public sealed class RegisterAccountValidatorTests
{
    private readonly RegisterAccountValidator validator = new();

    [Fact]
    public void Validate_CurrentTermsVersion_IsValid()
    {
        validator.Validate(new RegisterAccountCommand(LegalDocuments.CurrentTermsVersion)).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("2000-01-01")]
    public void Validate_OtherTermsVersion_IsInvalid(string version)
    {
        validator.Validate(new RegisterAccountCommand(version)).IsValid.Should().BeFalse();
    }
}
