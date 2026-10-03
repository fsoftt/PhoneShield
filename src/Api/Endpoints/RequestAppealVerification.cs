using MediatR;
using Tranqui.Api.RateLimiting;
using Tranqui.Application.Features.RequestAppealVerification;
using Tranqui.Contracts.Appeals;

namespace Tranqui.Api.Endpoints;

public static class RequestAppealVerification
{
    public const string Route = "/v1/appeals/verification-requests";

    public static IEndpointRouteBuilder MapRequestAppealVerification(this IEndpointRouteBuilder app)
    {
        // The app asks here before Firebase sends the SMS, so the number, account and device each stay within quota.
        app.MapPost(Route, async (AppealVerificationRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var phoneNumber = await sender.Send(
                    new RequestAppealVerificationCommand(request.PhoneNumber, request.DeviceId, request.IntegrityToken),
                    cancellationToken);

                return TypedResults.Ok(new AppealVerificationResponse(phoneNumber));
            })
            .RequireRateLimiting(RateLimitingExtensions.AppealPolicy)
            .WithName(nameof(RequestAppealVerification));

        return app;
    }
}
