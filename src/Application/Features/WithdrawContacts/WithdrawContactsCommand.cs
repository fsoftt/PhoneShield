using MediatR;

namespace Tranqui.Application.Features.WithdrawContacts;

/// <summary>Revokes the contact upload consent and deletes everything the user contributed from their address book.</summary>
public sealed record WithdrawContactsCommand : IRequest<int>;
