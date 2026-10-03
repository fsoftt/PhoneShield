namespace Tranqui.Contracts.Appeals;

/// <summary>
/// <see cref="PhoneProof"/> is the ID token of the Firebase SMS sign-in for the number; <see cref="IntegrityToken"/>
/// is a Play Integrity token requested with the appeal nonce for that number and this device.
/// </summary>
public sealed record SubmitAppealRequest(
    string PhoneProof,
    string DeviceId,
    string IntegrityToken,
    AppealKindDto Kind,
    string? Reason,
    string? ContactEmail);
