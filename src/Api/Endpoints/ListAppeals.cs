using MediatR;
using Tranqui.Api.BackOffice;
using Tranqui.Application.Features.ListAppeals;
using Tranqui.Contracts.Appeals;
using Tranqui.Contracts.BackOffice;
using Tranqui.Contracts.Lookups;
using Tranqui.Domain.Appeals;

namespace Tranqui.Api.Endpoints;

public static class ListAppeals
{
    public const string Route = "/v1/admin/appeals";

    public static IEndpointRouteBuilder MapListAppeals(this IEndpointRouteBuilder app)
    {
        app.MapGet(Route, async (AppealStatusDto? status, ISender sender, CancellationToken cancellationToken) =>
            {
                var reviews = await sender.Send(
                    new ListAppealsQuery((AppealStatus)(status ?? AppealStatusDto.Pending)), cancellationToken);

                return TypedResults.Ok(reviews.Select(review => new AppealReviewResponse(
                        review.Id,
                        (AppealKindDto)review.Kind,
                        (AppealStatusDto)review.Status,
                        review.Reason,
                        review.ContactEmail,
                        review.CreatedAt,
                        review.DueAt,
                        review.ResolvedAt,
                        review.IsOverdue,
                        (CallerStatusDto)review.NumberStatus,
                        review.SpamReportCount,
                        review.NotSpamReportCount,
                        review.SavedByCount))
                    .ToList());
            })
            .RequireAuthorization(BackOfficeExtensions.AdminPolicy)
            .WithName(nameof(ListAppeals));

        return app;
    }
}
