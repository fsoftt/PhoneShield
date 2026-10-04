using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tranqui.Api.RateLimiting;
using Tranqui.Application.Features.UnblockNumber;
using Tranqui.Contracts.Blocks;

namespace Tranqui.Api.Endpoints;

public static class UnblockNumber
{
    public static IEndpointRouteBuilder MapUnblockNumber(this IEndpointRouteBuilder app)
    {
        // The number travels in the body, never in the URL (request logging records paths).
        app.MapDelete(BlockNumber.Route, async ([FromBody] BlockRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new UnblockNumberCommand(request.PhoneNumber), cancellationToken);

                return TypedResults.NoContent();
            })
            .RequireRateLimiting(RateLimitingExtensions.BlockPolicy)
            .WithName(nameof(UnblockNumber));

        return app;
    }
}
