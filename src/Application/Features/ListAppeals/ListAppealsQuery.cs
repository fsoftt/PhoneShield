using MediatR;
using Tranqui.Domain.Appeals;

namespace Tranqui.Application.Features.ListAppeals;

/// <summary>Back office: appeals in one status, oldest first, with what the community currently says about the number.</summary>
public sealed record ListAppealsQuery(AppealStatus Status) : IRequest<IReadOnlyList<AppealReview>>;
