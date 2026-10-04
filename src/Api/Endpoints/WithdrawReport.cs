using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tranqui.Api.RateLimiting;
using Tranqui.Application.Features.WithdrawReport;
using Tranqui.Contracts.Reports;

namespace Tranqui.Api.Endpoints;

public static class WithdrawReport
{
    public static IEndpointRouteBuilder MapWithdrawReport(this IEndpointRouteBuilder app)
    {
        // The number travels in the body, never in the URL (request logging records paths).
        app.MapDelete(ReportCall.Route, async ([FromBody] WithdrawReportRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new WithdrawReportCommand(request.PhoneNumber), cancellationToken);

                return TypedResults.NoContent();
            })
            .RequireRateLimiting(RateLimitingExtensions.ReportPolicy)
            .WithName(nameof(WithdrawReport));

        return app;
    }
}
