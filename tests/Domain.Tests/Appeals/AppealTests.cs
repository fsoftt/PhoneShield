using System.Security.Cryptography;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Tests.Appeals;

public sealed class AppealTests
{
    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly PhoneHash hash = new(RandomNumberGenerator.GetBytes(PhoneHash.SizeInBytes), 1);

    [Fact]
    public void File_HideNames_IsAppliedRightAway()
    {
        Appeal.File(hash, AppealKind.HideNames, null, null, now).Status.Should().Be(AppealStatus.Applied);
    }

    [Fact]
    public void File_ReviewSpam_WaitsForAPerson()
    {
        Appeal.File(hash, AppealKind.ReviewSpam, "Mi tienda", null, now).Status.Should().Be(AppealStatus.Pending);
    }

    [Fact]
    public void File_BlankTexts_AreStoredAsNull()
    {
        var appeal = Appeal.File(hash, AppealKind.ReviewSpam, "  ", " ", now);

        appeal.Reason.Should().BeNull();
        appeal.ContactEmail.Should().BeNull();
    }

    [Fact]
    public void File_ReasonTooLong_Throws()
    {
        var act = () => Appeal.File(hash, AppealKind.ReviewSpam, new string('a', AppealRules.ReasonMaxLength + 1), null, now);

        act.Should().Throw<ArgumentException>();
    }
}
