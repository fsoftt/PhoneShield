using MediatR;
using Tranqui.Api.BackOffice;
using Tranqui.Application.Features.PurgeExpiredData;
using Tranqui.Contracts.BackOffice;

namespace Tranqui.Api.Endpoints;

public static class PurgeExpiredData
{
    public const string Route = "/v1/admin/purges";

    public static IEndpointRouteBuilder MapPurgeExpiredData(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, async (ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new PurgeExpiredDataCommand(), cancellationToken);

                return TypedResults.Ok(new PurgeResponse(result.AppealQuotaUsages, result.ResolvedAppeals, result.SpamReports));
            })
            .RequireAuthorization(BackOfficeExtensions.AdminPolicy)
            .WithName(nameof(PurgeExpiredData));

        return app;
    }
}
