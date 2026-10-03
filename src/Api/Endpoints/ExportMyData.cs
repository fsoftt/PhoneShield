using MediatR;
using Tranqui.Application.Features.ExportMyData;
using Tranqui.Contracts.Accounts;

namespace Tranqui.Api.Endpoints;

public static class ExportMyData
{
    public const string Route = "/v1/account/export";

    public static IEndpointRouteBuilder MapExportMyData(this IEndpointRouteBuilder app)
    {
        app.MapGet(Route, async (ISender sender, CancellationToken cancellationToken) =>
            {
                var export = await sender.Send(new ExportMyDataQuery(), cancellationToken);

                return TypedResults.Ok(new MyDataResponse(
                    export.AccountId,
                    export.CreatedAt,
                    export.Consents
                        .Select(consent => new ConsentResponse(consent.Type, consent.Version, consent.AcceptedAt, consent.RevokedAt))
                        .ToList(),
                    export.SpamReportCount,
                    export.ContactContributionCount));
            })
            .WithName(nameof(ExportMyData));

        return app;
    }
}
