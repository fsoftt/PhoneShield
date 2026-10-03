using MediatR;
using Tranqui.Domain.Appeals;

namespace Tranqui.Application.Features.SubmitAppeal;

/// <summary>
/// Filed from the app by the owner of a number. <see cref="PhoneProof"/> is the ID token of the SMS sign-in; the number
/// comes from it, never from the client.
/// </summary>
public sealed record SubmitAppealCommand(
    string PhoneProof,
    string DeviceId,
    string IntegrityToken,
    AppealKind Kind,
    string? Reason,
    string? ContactEmail) : IRequest<AppealStatus>;
