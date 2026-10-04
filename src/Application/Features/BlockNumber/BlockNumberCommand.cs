using MediatR;

namespace Tranqui.Application.Features.BlockNumber;

/// <summary>The user blocked a number on their phone; it counts as a weak spam signal. Idempotent.</summary>
public sealed record BlockNumberCommand(string PhoneNumber) : IRequest;
