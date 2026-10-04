using Microsoft.Extensions.Options;
using Tranqui.Api.BackOffice;
using Tranqui.Contracts.BackOffice;

namespace Tranqui.Api.Endpoints;

public static class GetBackOfficeConfig
{
    public const string Route = "/v1/admin/config";

    public static IEndpointRouteBuilder MapGetBackOfficeConfig(this IEndpointRouteBuilder app)
    {
        // Anonymous: the page needs the (public) Firebase Web API key before anyone can sign in.
        app.MapGet(Route, (IOptions<BackOfficeOptions> options) =>
                TypedResults.Ok(new BackOfficeConfigResponse(options.Value.FirebaseApiKey)))
            .AllowAnonymous()
            .WithName(nameof(GetBackOfficeConfig));

        return app;
    }
}
