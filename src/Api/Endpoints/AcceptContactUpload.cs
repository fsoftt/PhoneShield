using MediatR;
using Tranqui.Application.Features.AcceptContactUpload;
using Tranqui.Contracts.Accounts;

namespace Tranqui.Api.Endpoints;

public static class AcceptContactUpload
{
    public const string Route = "/v1/account/consents/contact-upload";

    public static IEndpointRouteBuilder MapAcceptContactUpload(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, async (AcceptContactUploadRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new AcceptContactUploadCommand(request.AcceptedVersion), cancellationToken);

                return TypedResults.NoContent();
            })
            .WithName(nameof(AcceptContactUpload));

        return app;
    }
}
