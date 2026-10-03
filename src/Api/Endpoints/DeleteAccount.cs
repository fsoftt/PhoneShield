using MediatR;
using Tranqui.Application.Features.DeleteAccount;

namespace Tranqui.Api.Endpoints;

public static class DeleteAccount
{
    public static IEndpointRouteBuilder MapDeleteAccount(this IEndpointRouteBuilder app)
    {
        app.MapDelete(RegisterAccount.Route, async (ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteAccountCommand(), cancellationToken);

                return TypedResults.NoContent();
            })
            .WithName(nameof(DeleteAccount));

        return app;
    }
}
