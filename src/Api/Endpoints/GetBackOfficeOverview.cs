using MediatR;
using Tranqui.Api.BackOffice;
using Tranqui.Application.Features.GetBackOfficeOverview;
using Tranqui.Contracts.BackOffice;

namespace Tranqui.Api.Endpoints;

public static class GetBackOfficeOverview
{
    public const string Route = "/v1/admin/overview";

    public static IEndpointRouteBuilder MapGetBackOfficeOverview(this IEndpointRouteBuilder app)
    {
        app.MapGet(Route, async (ISender sender, CancellationToken cancellationToken) =>
            {
                var overview = await sender.Send(new GetBackOfficeOverviewQuery(), cancellationToken);

                return TypedResults.Ok(new BackOfficeOverviewResponse(
                    overview.Accounts,
                    overview.SpamReports,
                    overview.ContactContributions,
                    overview.PendingAppeals,
                    overview.OverdueAppeals,
                    overview.HiddenNumbers,
                    overview.ClearedNumbers));
            })
            .RequireAuthorization(BackOfficeExtensions.AdminPolicy)
            .WithName(nameof(GetBackOfficeOverview));

        return app;
    }
}
