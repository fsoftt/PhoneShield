using MediatR;

namespace Tranqui.Application.Features.GetBackOfficeOverview;

public sealed record GetBackOfficeOverviewQuery : IRequest<BackOfficeOverview>;
