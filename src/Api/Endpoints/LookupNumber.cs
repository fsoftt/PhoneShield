using MediatR;
using Tranqui.Api.RateLimiting;
using Tranqui.Application.Features.LookupNumber;
using Tranqui.Contracts.Lookups;
using Tranqui.Domain.Reputation;

namespace Tranqui.Api.Endpoints;

public static class LookupNumber
{
    public const string Route = "/v1/lookups";

    public static IEndpointRouteBuilder MapLookupNumber(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, async (LookupRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new LookupNumberQuery(request.PhoneNumber), cancellationToken);

                return TypedResults.Ok(new LookupResponse(
                    ToDto(result.Status),
                    result.DisplayName,
                    result.OtherNames,
                    result.SpamReportCount,
                    result.SavedByCount));
            })
            .RequireRateLimiting(RateLimitingExtensions.LookupPolicy)
            .WithName(nameof(LookupNumber));

        return app;
    }

    private static CallerStatusDto ToDto(CallerStatus status) => status switch
    {
        CallerStatus.Spam => CallerStatusDto.Spam,
        CallerStatus.Identified => CallerStatusDto.Identified,
        _ => CallerStatusDto.Unknown,
    };
}
