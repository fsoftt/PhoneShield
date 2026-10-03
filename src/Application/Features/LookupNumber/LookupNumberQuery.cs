using MediatR;

namespace Tranqui.Application.Features.LookupNumber;

/// <summary>What the community knows about an incoming number. The lookup itself is never stored.</summary>
public sealed record LookupNumberQuery(string PhoneNumber) : IRequest<LookupResult>;
