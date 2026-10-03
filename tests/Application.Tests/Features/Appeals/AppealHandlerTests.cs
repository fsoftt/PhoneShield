using System.Security.Cryptography;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Tranqui.Application.Abstractions;
using Tranqui.Application.Appeals;
using Tranqui.Application.Errors;
using Tranqui.Application.Features.RequestAppealVerification;
using Tranqui.Application.Features.SubmitAppeal;
using Tranqui.Domain.Abstractions;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.Legal;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Tests.Features.Appeals;

public sealed class AppealHandlerTests
{
    private const string FirebaseUid = "firebase-uid-123";
    private const string Number = "3001234567";
    private const string E164 = "+573001234567";
    private const string Device = "9774d56d682e549c";
    private const string PhoneProof = "phone-proof";

    private static readonly DateTimeOffset now = new(2026, 10, 3, 12, 0, 0, TimeSpan.Zero);
    private static readonly PhoneHash hash = new(RandomNumberGenerator.GetBytes(PhoneHash.SizeInBytes), 1);

    private readonly IUserRepository users = Substitute.For<IUserRepository>();
    private readonly IDeviceIntegrityVerifier integrity = Substitute.For<IDeviceIntegrityVerifier>();
    private readonly IAppealRepository appeals = Substitute.For<IAppealRepository>();
    private readonly IPhoneProofValidator phoneProofs = Substitute.For<IPhoneProofValidator>();
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AppealGate gate;

    public AppealHandlerTests()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.FirebaseUid.Returns(FirebaseUid);
        var hasher = Substitute.For<IPhoneNumberHasher>();
        hasher.Hash(Arg.Any<PhoneNumber>()).Returns(hash);
        var contributorIds = Substitute.For<IContributorIdProvider>();
        contributorIds.FromFirebaseUid(FirebaseUid).Returns(new ContributorId(RandomNumberGenerator.GetBytes(ContributorId.SizeInBytes)));
        var deviceKeys = Substitute.For<IDeviceKeyProvider>();
        deviceKeys.FromDeviceId(Arg.Any<DeviceId>()).Returns(RandomNumberGenerator.GetBytes(32));
        integrity.IsTrustedAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        phoneProofs.VerifiedNumberAsync(PhoneProof, Arg.Any<CancellationToken>()).Returns(E164);
        GivenUserCreated(now - AppealRules.MinimumAccountAge);

        gate = new AppealGate(
            currentUser, users, integrity, hasher, contributorIds, deviceKeys, appeals, new FakeTimeProvider(now));
    }

    [Fact]
    public async Task RequestVerification_EstablishedAccountOnTrustedDevice_RecordsNumberAccountAndDevice()
    {
        var e164 = await RequestVerificationAsync();

        e164.Should().Be(E164);
        appeals.Received(3).AddUsage(Arg.Is<AppealQuotaUsage>(usage => usage.Action == AppealAction.SmsVerification));
        appeals.Received(1).AddUsage(Arg.Is<AppealQuotaUsage>(usage => usage.SubjectKind == QuotaSubjectKind.Number));
        appeals.Received(1).AddUsage(Arg.Is<AppealQuotaUsage>(usage => usage.SubjectKind == QuotaSubjectKind.Account));
        appeals.Received(1).AddUsage(Arg.Is<AppealQuotaUsage>(usage => usage.SubjectKind == QuotaSubjectKind.Device));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestVerification_ChecksIntegrityWithTheNonceForThisNumberAndDevice()
    {
        await RequestVerificationAsync();

        var expectedNonce = AppealNonce.Compute(
            AppealAction.SmsVerification, DeviceId.TryParse(Device)!, PhoneNumber.TryParse(Number)!);
        await integrity.Received(1).IsTrustedAsync("integrity-token", expectedNonce, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RequestVerification_UnregisteredAccount_Throws()
    {
        users.GetByFirebaseUidAsync(FirebaseUid, Arg.Any<CancellationToken>()).Returns((User?)null);

        await FluentActions.Awaiting(RequestVerificationAsync).Should().ThrowAsync<AccountNotRegisteredException>();
    }

    [Fact]
    public async Task RequestVerification_NewAccount_Throws()
    {
        GivenUserCreated(now.AddDays(-1));

        await FluentActions.Awaiting(RequestVerificationAsync).Should().ThrowAsync<AppealAccountTooNewException>();
    }

    [Fact]
    public async Task RequestVerification_UntrustedDevice_ThrowsAndRecordsNothing()
    {
        integrity.IsTrustedAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        await FluentActions.Awaiting(RequestVerificationAsync).Should().ThrowAsync<DeviceNotTrustedException>();
        appeals.DidNotReceive().AddUsage(Arg.Any<AppealQuotaUsage>());
    }

    [Theory]
    [InlineData(QuotaSubjectKind.Number)]
    [InlineData(QuotaSubjectKind.Account)]
    [InlineData(QuotaSubjectKind.Device)]
    public async Task RequestVerification_AnySubjectUsedItsMonth_ThrowsAndRecordsNothing(QuotaSubjectKind exhausted)
    {
        appeals.CountUsagesSinceAsync(
                Arg.Is<QuotaSubject>(subject => subject.Kind == exhausted),
                AppealAction.SmsVerification,
                Arg.Any<DateTimeOffset>(),
                Arg.Any<CancellationToken>())
            .Returns(1);

        await FluentActions.Awaiting(RequestVerificationAsync).Should().ThrowAsync<AppealLimitReachedException>();
        appeals.DidNotReceive().AddUsage(Arg.Any<AppealQuotaUsage>());
    }

    [Fact]
    public async Task Submit_UnverifiedPhone_Throws()
    {
        var act = () => SubmitHandler().Handle(Submission(AppealKind.HideNames) with { PhoneProof = "forged" }, CancellationToken.None);

        await act.Should().ThrowAsync<PhoneNotVerifiedException>();
    }

    [Fact]
    public async Task Submit_HideNames_HidesTheNumber()
    {
        var status = await SubmitHandler().Handle(Submission(AppealKind.HideNames), CancellationToken.None);

        status.Should().Be(AppealStatus.Applied);
        appeals.Received(1).Hide(Arg.Any<HiddenNumber>());
        appeals.Received(3).AddUsage(Arg.Is<AppealQuotaUsage>(usage => usage.Action == AppealAction.Appeal));
    }

    [Fact]
    public async Task Submit_HideNamesForAHiddenNumber_DoesNotHideTwice()
    {
        appeals.IsHiddenAsync(hash, Arg.Any<CancellationToken>()).Returns(true);

        await SubmitHandler().Handle(Submission(AppealKind.HideNames), CancellationToken.None);

        appeals.DidNotReceive().Hide(Arg.Any<HiddenNumber>());
    }

    [Fact]
    public async Task Submit_ReviewSpam_IsPendingAndHidesNothing()
    {
        var status = await SubmitHandler().Handle(Submission(AppealKind.ReviewSpam), CancellationToken.None);

        status.Should().Be(AppealStatus.Pending);
        appeals.DidNotReceive().Hide(Arg.Any<HiddenNumber>());
    }

    private Task<string> RequestVerificationAsync() =>
        new RequestAppealVerificationHandler(gate, unitOfWork)
            .Handle(new RequestAppealVerificationCommand(Number, Device, "integrity-token"), CancellationToken.None);

    private SubmitAppealHandler SubmitHandler() =>
        new(phoneProofs, gate, appeals, unitOfWork, new FakeTimeProvider(now));

    private static SubmitAppealCommand Submission(AppealKind kind) =>
        new(PhoneProof, Device, "integrity-token", kind, null, null);

    private void GivenUserCreated(DateTimeOffset createdAt) =>
        users.GetByFirebaseUidAsync(FirebaseUid, Arg.Any<CancellationToken>())
            .Returns(User.Register(FirebaseUid, LegalDocuments.CurrentTermsVersion, createdAt));
}
