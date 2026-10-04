using MediatR;
using Tranqui.Api.RateLimiting;
using Tranqui.Application.Features.BlockNumber;
using Tranqui.Contracts.Blocks;

namespace Tranqui.Api.Endpoints;

public static class BlockNumber
{
    public const string Route = "/v1/blocks";

    public static IEndpointRouteBuilder MapBlockNumber(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, async (BlockRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new BlockNumberCommand(request.PhoneNumber), cancellationToken);

                return TypedResults.NoContent();
            })
            .RequireRateLimiting(RateLimitingExtensions.BlockPolicy)
            .WithName(nameof(BlockNumber));

        return app;
    }
}
