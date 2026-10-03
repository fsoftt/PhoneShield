using System.Security.Cryptography;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Domain.Tests.Reputation;

public sealed class SpamReportTests
{
    private static readonly PhoneHash hash = new(RandomNumberGenerator.GetBytes(PhoneHash.SizeInBytes), 1);
    private static readonly ContributorId contributor = new(RandomNumberGenerator.GetBytes(ContributorId.SizeInBytes));
    private static readonly ProtectedName label = new([1], [1]);

    [Fact]
    public void Create_SpamWithLabel_KeepsTheLabel()
    {
        var report = SpamReport.Create(hash, contributor, ReportVerdict.Spam, 1, label, DateTimeOffset.UtcNow);

        report.Label.Should().Be(label);
        report.PhoneHash.Should().Equal(hash.Value.ToArray());
        report.HashKeyVersion.Should().Be(hash.KeyVersion);
    }

    [Fact]
    public void Create_NotSpamWithLabel_Throws()
    {
        var act = () => SpamReport.Create(hash, contributor, ReportVerdict.NotSpam, 1, label, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_NonPositiveWeight_Throws()
    {
        var act = () => SpamReport.Create(hash, contributor, ReportVerdict.Spam, 0, null, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Revise_ReplacesTheVerdictAndDropsTheLabel()
    {
        var report = SpamReport.Create(hash, contributor, ReportVerdict.Spam, 1, label, DateTimeOffset.UtcNow);
        var revisedAt = DateTimeOffset.UtcNow.AddDays(1);

        report.Revise(ReportVerdict.NotSpam, 0.5, null, revisedAt);

        report.Verdict.Should().Be(ReportVerdict.NotSpam);
        report.Weight.Should().Be(0.5);
        report.Label.Should().BeNull();
        report.ReportedAt.Should().Be(revisedAt);
    }

    [Fact]
    public void Revise_NotSpamWithLabel_Throws()
    {
        var report = SpamReport.Create(hash, contributor, ReportVerdict.Spam, 1, null, DateTimeOffset.UtcNow);

        var act = () => report.Revise(ReportVerdict.NotSpam, 1, label, DateTimeOffset.UtcNow);

        act.Should().Throw<ArgumentException>();
    }
}
