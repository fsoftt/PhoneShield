using MediatR;

namespace Tranqui.Application.Features.DeleteAccount;

/// <summary>
/// Right to erasure: deletes the account, its consents, reports and contact contributions. The app deletes the
/// Firebase identity afterwards, so the server never needs Firebase admin credentials. Idempotent.
/// </summary>
public sealed record DeleteAccountCommand : IRequest;
