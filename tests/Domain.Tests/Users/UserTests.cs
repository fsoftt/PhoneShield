using Tranqui.Domain.Legal;
using Tranqui.Domain.Users;

namespace Tranqui.Domain.Tests.Users;

public sealed class UserTests
{
    private const string FirebaseUid = "firebase-uid-123";

    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_WithCurrentTerms_RecordsTheConsent()
    {
        var user = User.Register(FirebaseUid, LegalDocuments.CurrentTermsVersion, now);

        user.FirebaseUid.Should().Be(FirebaseUid);
        user.CreatedAt.Should().Be(now);
        user.Id.Should().NotBe(Guid.Empty);
        user.Consents.Should().ContainSingle(consent =>
            consent.Type == ConsentType.TermsAndPrivacyPolicy
            && consent.Version == LegalDocuments.CurrentTermsVersion
            && consent.AcceptedAt == now);
        user.AcceptedVersionOf(ConsentType.TermsAndPrivacyPolicy).Should().Be(LegalDocuments.CurrentTermsVersion);
    }

    [Fact]
    public void Register_WithOutdatedTerms_Throws()
    {
        var act = () => User.Register(FirebaseUid, "2000-01-01", now);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_WithoutFirebaseUid_Throws(string firebaseUid)
    {
        var act = () => User.Register(firebaseUid, LegalDocuments.CurrentTermsVersion, now);

        act.Should().Throw<ArgumentException>();
    }
}
