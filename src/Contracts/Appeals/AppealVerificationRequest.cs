namespace Tranqui.Contracts.Appeals;

/// <summary>
/// <see cref="IntegrityToken"/> is a Play Integrity token requested with the appeal nonce for this number and device.
/// </summary>
public sealed record AppealVerificationRequest(string PhoneNumber, string DeviceId, string IntegrityToken);
