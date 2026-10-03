using Tranqui.Application.Abstractions;
using Tranqui.Application.Errors;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;
using Tranqui.Domain.Users;

namespace Tranqui.Application.Appeals;

/// <summary>
/// Checks shared by every appeal step: an established account, a genuine app on a genuine device (Play Integrity),
/// and quota left for the number, the account and the device. Records the use; the caller saves.
/// </summary>
internal sealed class AppealGate(
    ICurrentUser currentUser,
    IUserRepository users,
    IDeviceIntegrityVerifier integrityVerifier,
    IPhoneNumberHasher hasher,
    IContributorIdProvider contributorIds,
    IDeviceKeyProvider deviceKeys,
    IAppealRepository appeals,
    TimeProvider timeProvider)
{
    public async Task<PhoneHash> ConsumeAsync(
        AppealAction action,
        PhoneNumber phoneNumber,
        DeviceId deviceId,
        string integrityToken,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var user = await users.GetByFirebaseUidAsync(currentUser.FirebaseUid, cancellationToken)
            ?? throw new AccountNotRegisteredException();
        if (now - user.CreatedAt < AppealRules.MinimumAccountAge)
        {
            throw new AppealAccountTooNewException();
        }

        if (!await integrityVerifier.IsTrustedAsync(integrityToken, AppealNonce.Compute(action, deviceId, phoneNumber), cancellationToken))
        {
            throw new DeviceNotTrustedException();
        }

        var phoneHash = hasher.Hash(phoneNumber);
        QuotaSubject[] subjects =
        [
            new(QuotaSubjectKind.Number, phoneHash.Value.ToArray()),
            new(QuotaSubjectKind.Account, contributorIds.FromFirebaseUid(currentUser.FirebaseUid).Value.ToArray()),
            new(QuotaSubjectKind.Device, deviceKeys.FromDeviceId(deviceId)),
        ];

        foreach (var subject in subjects)
        {
            var lastMonth = await appeals.CountUsagesSinceAsync(subject, action, now - AppealRules.MonthWindow, cancellationToken);
            var lastYear = await appeals.CountUsagesSinceAsync(subject, action, now - AppealRules.YearWindow, cancellationToken);
            if (!AppealRules.Allows(subject.Kind, lastMonth, lastYear))
            {
                throw new AppealLimitReachedException();
            }
        }

        foreach (var subject in subjects)
        {
            appeals.AddUsage(AppealQuotaUsage.Record(subject, action, now));
        }

        return phoneHash;
    }
}
