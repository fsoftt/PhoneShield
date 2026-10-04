using MediatR;

namespace Tranqui.Application.Features.UnblockNumber;

/// <summary>The user unblocked a number: their block signal is removed. Idempotent.</summary>
public sealed record UnblockNumberCommand(string PhoneNumber) : IRequest;
