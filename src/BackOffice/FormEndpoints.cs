using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Tranqui.BackOffice.Api;
using Tranqui.BackOffice.Authentication;
using Tranqui.Contracts.BackOffice;

namespace Tranqui.BackOffice;

/// <summary>
/// The back office's actions are plain HTML form posts (antiforgery-checked), each answered with a redirect back to
/// the page (post/redirect/get), so no JavaScript is needed and a refresh never repeats an action.
/// </summary>
internal static class FormEndpoints
{
    public static IEndpointRouteBuilder MapFormEndpoints(this IEndpointRouteBuilder app)
    {
        // Every action requires a valid antiforgery token. The middleware validates it, but minimal APIs only reject an
        // invalid one when they bind form fields; logout and purge have none, so the filter rejects it for all.
        var forms = app.MapGroup(string.Empty)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute())
            .AddEndpointFilter(async (context, next) =>
                context.HttpContext.Features.Get<IAntiforgeryValidationFeature>() is { IsValid: true }
                    ? await next(context)
                    // A body, so the status code pages do not re-execute the request as a POST to another page.
                    : Results.BadRequest("The form expired. Reload the page and try again."));

        forms.MapPost(BackOfficeRoutes.Logout, async (HttpContext context) =>
        {
            await context.SignOutAsync(SessionCookie.Scheme);
            return Results.LocalRedirect(BackOfficeRoutes.Login);
        });

        forms.MapPost("/appeals/{id:guid}/resolution", async (
            Guid id,
            [FromForm] AppealDecisionDto decision,
            [FromForm] string? returnStatus,
            AdminApiClient api,
            CancellationToken cancellationToken) =>
        {
            var back = $"{BackOfficeRoutes.Appeals}?status={Uri.EscapeDataString(returnStatus ?? "Pending")}";
            var notice = await RunAsync(() => api.ResolveAsync(id, decision, cancellationToken), Notices.Resolved);

            return Results.LocalRedirect($"{back}&notice={notice}");
        });

        forms.MapPost(BackOfficeRoutes.Purge, async (AdminApiClient api, CancellationToken cancellationToken) =>
        {
            PurgeResponse? result = null;
            var notice = await RunAsync(async () => result = await api.PurgeAsync(cancellationToken), Notices.Purged);
            var counts = result is null ? string.Empty
                : $"&quotaUses={result.AppealQuotaUsages}&appeals={result.ResolvedAppeals}&reports={result.SpamReports}&blocks={result.BlockSignals}";

            return Results.LocalRedirect($"{BackOfficeRoutes.Home}?notice={notice}{counts}");
        });

        return app;
    }

    private static async Task<string> RunAsync(Func<Task> action, string successNotice)
    {
        try
        {
            await action();
            return successNotice;
        }
        catch (AdminApiException exception) when (exception.IsForbidden)
        {
            return Notices.Forbidden;
        }
        catch (Exception exception) when (exception is AdminApiException or HttpRequestException or TaskCanceledException)
        {
            return Notices.Error;
        }
    }
}
