using MediatR;
using Tranqui.Api.BackOffice;
using Tranqui.Application.Features.ResolveAppeal;
using Tranqui.Contracts.Appeals;
using Tranqui.Contracts.BackOffice;
using Tranqui.Domain.Appeals;

namespace Tranqui.Api.Endpoints;

public static class ResolveAppeal
{
    public const string Route = "/v1/admin/appeals/{id:guid}/resolution";

    public static string RouteFor(Guid id) => Route.Replace("{id:guid}", id.ToString(), StringComparison.Ordinal);

    public static IEndpointRouteBuilder MapResolveAppeal(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, async (Guid id, ResolveAppealRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var status = await sender.Send(new ResolveAppealCommand(id, (AppealDecision)request.Decision), cancellationToken);

                return TypedResults.Ok(new AppealResponse((AppealStatusDto)status));
            })
            .RequireAuthorization(BackOfficeExtensions.AdminPolicy)
            .WithName(nameof(ResolveAppeal));

        return app;
    }
}
