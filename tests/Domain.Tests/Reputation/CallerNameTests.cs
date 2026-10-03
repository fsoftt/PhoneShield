using Tranqui.Domain.Reputation;

namespace Tranqui.Domain.Tests.Reputation;

public sealed class CallerNameTests
{
    [Fact]
    public void TryCreate_CollapsesWhitespaceForDisplay()
    {
        CallerName.TryCreate("  Spam   Claro ")!.DisplayValue.Should().Be("Spam Claro");
    }

    [Theory]
    [InlineData("Spam Claro")]
    [InlineData("spam claro")]
    [InlineData("SPAM  CLARO")]
    public void TryCreate_SameNameDifferentCasing_SharesCanonicalValue(string raw)
    {
        CallerName.TryCreate(raw)!.CanonicalValue.Should().Be("spam claro");
    }

    [Fact]
    public void TryCreate_IgnoresAccentsWhenGrouping()
    {
        CallerName.TryCreate("José Pérez")!.CanonicalValue.Should().Be(CallerName.TryCreate("jose perez")!.CanonicalValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryCreate_Blank_ReturnsNull(string? raw)
    {
        CallerName.TryCreate(raw).Should().BeNull();
    }

    [Fact]
    public void TryCreate_TooLong_ReturnsNull()
    {
        CallerName.TryCreate(new string('a', CallerName.MaxLength + 1)).Should().BeNull();
    }
}
