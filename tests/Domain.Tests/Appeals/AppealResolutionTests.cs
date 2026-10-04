using System.Security.Cryptography;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Domain.Tests.Appeals;

public sealed class AppealResolutionTests
{
    // A Friday, so the deadline has to skip weekends.
    private static readonly DateTimeOffset friday = new(2026, 10, 2, 12, 0, 0, TimeSpan.Zero);
    private static readonly PhoneHash hash = new(RandomNumberGenerator.GetBytes(PhoneHash.SizeInBytes), 2);

    [Fact]
    public void Resolve_ErasesTheReasonAndEmail()
    {
        var appeal = Appeal.File(hash, AppealKind.ReviewSpam, "Es mi tienda", "owner@example.com", friday);

        appeal.Resolve(AppealDecision.Approve, friday.AddDays(3));

        appeal.Status.Should().Be(AppealStatus.Approved);
        appeal.ResolvedAt.Should().Be(friday.AddDays(3));
        appeal.Reason.Should().BeNull();
        appeal.ContactEmail.Should().BeNull();
    }

    [Fact]
    public void Resolve_Reject_IsRejected()
    {
        var appeal = Appeal.File(hash, AppealKind.ReviewSpam, null, null, friday);

        appeal.Resolve(AppealDecision.Reject, friday);

        appeal.Status.Should().Be(AppealStatus.Rejected);
    }

    [Fact]
    public void Resolve_AlreadyApplied_Throws()
    {
        var appeal = Appeal.File(hash, AppealKind.HideNames, null, null, friday);

        appeal.Invoking(a => a.Resolve(AppealDecision.Approve, friday)).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Hash_KeepsTheKeyVersion()
    {
        Appeal.File(hash, AppealKind.ReviewSpam, null, null, friday).Hash.Should().Be(hash);
    }

    [Fact]
    public void DueAt_IsFifteenBusinessDaysLater()
    {
        // 15 business days from Friday Oct 2: three full weeks, landing on Friday Oct 23.
        Appeal.File(hash, AppealKind.ReviewSpam, null, null, friday).DueAt.Should().Be(friday.AddDays(21));
    }

    [Fact]
    public void BusinessDays_SkipWeekends()
    {
        BusinessDays.Add(friday, 1).DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Fact]
    public void WithSpamClearedBefore_DropsOnlyEarlierSpamVotes()
    {
        var signals = new ReputationSignals(
            [
                new SpamVote(ReportVerdict.Spam, 1, friday.AddDays(-1)),
                new SpamVote(ReportVerdict.NotSpam, 1, friday.AddDays(-1)),
                new SpamVote(ReportVerdict.Spam, 1, friday.AddDays(1)),
            ],
            [],
            0);

        var cleared = signals.WithSpamClearedBefore(friday);

        cleared.SpamVotes.Should().HaveCount(2).And.NotContain(vote => vote.Verdict == ReportVerdict.Spam && vote.CastAt < friday);
    }
}
