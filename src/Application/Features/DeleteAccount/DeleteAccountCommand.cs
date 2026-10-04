using MediatR;

namespace Tranqui.Application.Features.DeleteAccount;

/// <summary>
/// Right to erasure: deletes the account, its consents, reports and contact contributions. Shared blocks are kept by
/// default as a spam signal no longer tied to anyone (their contributor id is a keyed hash of a Firebase uid that is
/// deleted right after), unless <see cref="RemoveSharedBlocks"/> is set. The app deletes the Firebase identity
/// afterwards, so the server never needs Firebase admin credentials. Idempotent.
/// </summary>
public sealed record DeleteAccountCommand(bool RemoveSharedBlocks = false) : IRequest;
