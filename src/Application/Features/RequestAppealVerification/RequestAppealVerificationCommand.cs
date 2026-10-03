using MediatR;

namespace Tranqui.Application.Features.RequestAppealVerification;

/// <summary>
/// Asked by the app before Firebase sends the verification SMS, so the number, the account and the device each get
/// at most their SMS quota. Returns the normalized number the app must verify.
/// </summary>
public sealed record RequestAppealVerificationCommand(string PhoneNumber, string DeviceId, string IntegrityToken)
    : IRequest<string>;
