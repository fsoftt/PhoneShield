using Tranqui.App.Core.Api;
using Tranqui.App.Core.Authentication;
using Tranqui.Contracts.Appeals;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Appeals;

/// <summary>
/// An appeal for the user's own number: the API checks the quotas and the device, Firebase sends the SMS, and the SMS
/// sign-in goes to the API as proof of the number. That throwaway Firebase user is deleted right after.
/// </summary>
public sealed class AppealService(ITranquiApi api, IFirebaseAuthClient firebase, IDeviceIntegrity device)
{
    public async Task<AppealCodeRequest> RequestCodeAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);

        var integrityToken = await IntegrityTokenAsync(AppealAction.SmsVerification, phoneNumber, cancellationToken);
        var verified = await api.RequestAppealVerificationAsync(
            new AppealVerificationRequest(phoneNumber.E164, device.DeviceId, integrityToken), cancellationToken);

        var firebaseToken = await device.RequestTokenAsync(FirebasePhoneNonce.For(verified.PhoneNumber), cancellationToken);
        var sessionInfo = await firebase.SendVerificationCodeAsync(verified.PhoneNumber, firebaseToken, cancellationToken);

        return new AppealCodeRequest(verified.PhoneNumber, sessionInfo);
    }

    public async Task<AppealStatusDto> SubmitAsync(
        AppealCodeRequest codeRequest,
        string code,
        AppealKindDto kind,
        string? reason,
        string? contactEmail,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(codeRequest);

        var phoneProof = await firebase.SignInWithPhoneNumberAsync(codeRequest.SessionInfo, code, cancellationToken);
        try
        {
            var phoneNumber = PhoneNumber.TryParse(codeRequest.PhoneNumber)
                ?? throw new InvalidOperationException("The API returned a number it normalized itself.");
            var integrityToken = await IntegrityTokenAsync(AppealAction.Appeal, phoneNumber, cancellationToken);
            var response = await api.SubmitAppealAsync(
                new SubmitAppealRequest(phoneProof, device.DeviceId, integrityToken, kind, reason, contactEmail),
                cancellationToken);

            return response.Status;
        }
        finally
        {
            await firebase.DeleteAccountAsync(phoneProof, CancellationToken.None);
        }
    }

    private Task<string> IntegrityTokenAsync(AppealAction action, PhoneNumber phoneNumber, CancellationToken cancellationToken)
    {
        var deviceId = DeviceId.TryParse(device.DeviceId)
            ?? throw new InvalidOperationException("The platform returned an invalid device id.");

        return device.RequestTokenAsync(AppealNonce.Compute(action, deviceId, phoneNumber), cancellationToken);
    }
}
