using MediatR;
using Tranqui.Api.RateLimiting;
using Tranqui.Application.Features.SubmitAppeal;
using Tranqui.Contracts.Appeals;
using Tranqui.Domain.Appeals;

namespace Tranqui.Api.Endpoints;

public static class SubmitAppeal
{
    public const string Route = "/v1/appeals";

    public static IEndpointRouteBuilder MapSubmitAppeal(this IEndpointRouteBuilder app)
    {
        // The number comes from the SMS sign-in proof, never from the body.
        // The DTO enums mirror the domain values; out-of-range numbers are rejected by the validator.
        app.MapPost(Route, async (SubmitAppealRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var status = await sender.Send(
                    new SubmitAppealCommand(
                        request.PhoneProof,
                        request.DeviceId,
                        request.IntegrityToken,
                        (AppealKind)request.Kind,
                        request.Reason,
                        request.ContactEmail),
                    cancellationToken);

                return TypedResults.Ok(new AppealResponse((AppealStatusDto)status));
            })
            .RequireRateLimiting(RateLimitingExtensions.AppealPolicy)
            .WithName(nameof(SubmitAppeal));

        return app;
    }
}
