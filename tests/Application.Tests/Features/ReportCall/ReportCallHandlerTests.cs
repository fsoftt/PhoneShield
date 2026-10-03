using System.Security.Cryptography;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Application.Features.ReportCall;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Legal;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Tests.Features.ReportCall;

public sealed class ReportCallHandlerTests
{
    private const string FirebaseUid = "firebase-uid-123";
    private const string Number = "3001234567";

    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly PhoneHash hash = new(RandomNumberGenerator.GetBytes(PhoneHash.SizeInBytes), 1);
    private static readonly ContributorId contributor = new(RandomNumberGenerator.GetBytes(ContributorId.SizeInBytes));
    private static readonly ProtectedName protectedLabel = new([1], [1]);

    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly INameProtector nameProtector = Substitute.For<INameProtector>();
    private readonly ISpamReportRepository reports = Substitute.For<ISpamReportRepository>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ReportCallHandler handler;

    public ReportCallHandlerTests()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.FirebaseUid.Returns(FirebaseUid);
        var hasher = Substitute.For<IPhoneNumberHasher>();
        hasher.Hash(Arg.Any<PhoneNumber>()).Returns(hash);
        var contributorIds = Substitute.For<IContributorIdProvider>();
        contributorIds.FromFirebaseUid(FirebaseUid).Returns(contributor);
        nameProtector.Protect(Arg.Any<PhoneNumber>(), Arg.Any<CallerName>()).Returns(protectedLabel);

        handler = new ReportCallHandler(
            currentUser, users, hasher, nameProtector, contributorIds, reports, unitOfWork, new FakeTimeProvider(now));
    }

    [Fact]
    public async Task Handle_UnregisteredAccount_Throws()
    {
        var act = () => handler.Handle(new ReportCallCommand(Number, ReportVerdict.Spam, null), CancellationToken.None);

        await act.Should().ThrowAsync<AccountNotRegisteredException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_FirstReportFromNewAccount_AddsItWithReducedWeightAndProtectedLabel()
    {
        GivenUserCreated(now.AddDays(-1));

        await handler.Handle(new ReportCallCommand(Number, ReportVerdict.Spam, "Spam Claro"), CancellationToken.None);

        reports.Received(1).Add(Arg.Is<SpamReport>(report =>
            report.Verdict == ReportVerdict.Spam
            && report.Weight == ReputationRules.NewAccountVoteWeight
            && report.Label == protectedLabel));
        nameProtector.Received(1).Protect(
            Arg.Is<PhoneNumber>(number => number.E164 == "+573001234567"),
            Arg.Is<CallerName>(name => name.DisplayValue == "Spam Claro"));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RepeatedReport_RevisesTheExistingOne()
    {
        GivenUserCreated(now.AddDays(-30));
        var existing = SpamReport.Create(hash, contributor, ReportVerdict.Spam, 1, null, now.AddDays(-2));
        reports.GetAsync(hash, contributor, Arg.Any<CancellationToken>()).Returns(existing);

        await handler.Handle(new ReportCallCommand(Number, ReportVerdict.NotSpam, null), CancellationToken.None);

        reports.DidNotReceive().Add(Arg.Any<SpamReport>());
        existing.Verdict.Should().Be(ReportVerdict.NotSpam);
        existing.Weight.Should().Be(ReputationRules.EstablishedAccountVoteWeight);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private void GivenUserCreated(DateTimeOffset createdAt) =>
        users.GetByFirebaseUidAsync(FirebaseUid, Arg.Any<CancellationToken>())
            .Returns(User.Register(FirebaseUid, LegalDocuments.CurrentTermsVersion, createdAt));
}
