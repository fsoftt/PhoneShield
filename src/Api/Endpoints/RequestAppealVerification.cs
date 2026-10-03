using MediatR;
using Tranqui.Api.Hosting;
using Tranqui.Api.RateLimiting;
using Tranqui.Application.Features.RequestAppealVerification;
using Tranqui.Contracts.Appeals;

namespace Tranqui.Api.Endpoints;

public static class RequestAppealVerification
{
    public const string Route = "/v1/appeals/verification-requests";

    public static IEndpointRouteBuilder MapRequestAppealVerification(this IEndpointRouteBuilder app)
    {
        // Anonymous: the website asks here before Firebase sends the SMS, so each number gets one SMS per month.
        app.MapPost(Route, async (AppealVerificationRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var phoneNumber = await sender.Send(new RequestAppealVerificationCommand(request.PhoneNumber), cancellationToken);

                return TypedResults.Ok(new AppealVerificationResponse(phoneNumber));
            })
            .AllowAnonymous()
            .RequireCors(WebsiteCorsExtensions.WebsitePolicy)
            .RequireRateLimiting(RateLimitingExtensions.AppealPolicy)
            .WithName(nameof(RequestAppealVerification));

        return app;
    }
}
