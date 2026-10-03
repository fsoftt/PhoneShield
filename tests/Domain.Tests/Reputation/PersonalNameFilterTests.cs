using Tranqui.Domain.Reputation;

namespace Tranqui.Domain.Tests.Reputation;

public sealed class PersonalNameFilterTests
{
    [Theory]
    [InlineData("Mamá")]
    [InlineData("MAMA")]
    [InlineData("Mi amor")]
    [InlineData("Jefe")]
    [InlineData("mi ex")]
    [InlineData("Cuñada")]
    [InlineData("❤️")]
    [InlineData("J")]
    public void IsPersonal_RelationshipOrNonNames_AreFiltered(string raw)
    {
        PersonalNameFilter.IsPersonal(CallerName.TryCreate(raw)!).Should().BeTrue();
    }

    [Theory]
    [InlineData("Pizzería Juan")]
    [InlineData("Spam Claro")]
    [InlineData("Ana María")]
    [InlineData("Taxi Libre")]
    public void IsPersonal_CallerNames_AreKept(string raw)
    {
        PersonalNameFilter.IsPersonal(CallerName.TryCreate(raw)!).Should().BeFalse();
    }
}
