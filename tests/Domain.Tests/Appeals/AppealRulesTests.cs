using Tranqui.Domain.Appeals;

namespace Tranqui.Domain.Tests.Appeals;

public sealed class AppealRulesTests
{
    [Theory]
    [InlineData(QuotaSubjectKind.Number, 0, 0, true)]
    [InlineData(QuotaSubjectKind.Number, 1, 1, false)]
    [InlineData(QuotaSubjectKind.Number, 0, 11, true)]
    [InlineData(QuotaSubjectKind.Account, 0, 2, true)]
    [InlineData(QuotaSubjectKind.Account, 1, 1, false)]
    [InlineData(QuotaSubjectKind.Account, 0, 3, false)]
    [InlineData(QuotaSubjectKind.Device, 0, 3, false)]
    public void Allows_OnePerMonthAndThreePerYearForAccountsAndDevices(
        QuotaSubjectKind kind, int usedLastMonth, int usedLastYear, bool allowed)
    {
        AppealRules.Allows(kind, usedLastMonth, usedLastYear).Should().Be(allowed);
    }
}
