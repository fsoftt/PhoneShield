using Tranqui.Application.Features.ReportCall;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Tests.Features.ReportCall;

public sealed class ReportCallValidatorTests
{
    private const string ValidNumber = "3001234567";

    private readonly ReportCallValidator validator = new();

    [Theory]
    [InlineData(ReportVerdict.Spam, null)]
    [InlineData(ReportVerdict.Spam, "Spam Claro")]
    [InlineData(ReportVerdict.NotSpam, null)]
    public void Validate_ValidReport_IsValid(ReportVerdict verdict, string? label)
    {
        validator.Validate(new ReportCallCommand(ValidNumber, verdict, label)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidNumber_IsInvalid()
    {
        validator.Validate(new ReportCallCommand("123", ReportVerdict.Spam, null)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_UndefinedVerdict_IsInvalid()
    {
        validator.Validate(new ReportCallCommand(ValidNumber, (ReportVerdict)99, null)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_NotSpamWithLabel_IsInvalid()
    {
        validator.Validate(new ReportCallCommand(ValidNumber, ReportVerdict.NotSpam, "Pizzería")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_LabelTooLong_IsInvalid()
    {
        var label = new string('a', CallerName.MaxLength + 1);

        validator.Validate(new ReportCallCommand(ValidNumber, ReportVerdict.Spam, label)).IsValid.Should().BeFalse();
    }
}
