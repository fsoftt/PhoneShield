using Tranqui.Domain.Reputation;

namespace Tranqui.Domain.Tests.Reputation;

public sealed class CallerNameFilterTests
{
    [Theory]
    [InlineData("Spam HP")]
    [InlineData("gonorrea de banco")]
    [InlineData("Malparidos del call center")]
    [InlineData("Fucking telemarketer")]
    [InlineData("Mamá")]
    public void IsShareable_PersonalOrOffensive_IsFalse(string raw)
    {
        CallerNameFilter.IsShareable(CallerName.TryCreate(raw)!).Should().BeFalse();
    }

    [Theory]
    [InlineData("Spam Claro")]
    [InlineData("Dr. Hpérez")]
    [InlineData("Culinaria Express")]
    [InlineData("Marisol Peluquería")]
    [InlineData("Dickens Bookstore")]
    public void IsShareable_OrdinaryNames_IsTrue(string raw)
    {
        CallerNameFilter.IsShareable(CallerName.TryCreate(raw)!).Should().BeTrue();
    }
}
