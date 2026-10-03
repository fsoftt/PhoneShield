using MediatR;

namespace Tranqui.Application.Features.RegisterAccount;

/// <summary>Creates the account for the authenticated Firebase user. Idempotent: repeating it returns the same account.</summary>
public sealed record RegisterAccountCommand(string AcceptedTermsVersion) : IRequest<AccountResult>;
