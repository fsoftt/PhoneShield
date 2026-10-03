using MediatR;
using Tranqui.Api.RateLimiting;
using Tranqui.Application.Features.ReportCall;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.Reputation;

namespace Tranqui.Api.Endpoints;

public static class ReportCall
{
    public const string Route = "/v1/reports";

    public static IEndpointRouteBuilder MapReportCall(this IEndpointRouteBuilder app)
    {
        // ReportVerdictDto mirrors ReportVerdict's values; out-of-range numbers are rejected by the validator.
        app.MapPost(Route, async (ReportCallRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new ReportCallCommand(request.PhoneNumber, (ReportVerdict)request.Verdict, request.Label), cancellationToken);

                return TypedResults.NoContent();
            })
            .RequireRateLimiting(RateLimitingExtensions.ReportPolicy)
            .WithName(nameof(ReportCall));

        return app;
    }
}
