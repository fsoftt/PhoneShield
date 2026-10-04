using MediatR;
using Tranqui.Application.Features.DeleteAccount;

namespace Tranqui.Api.Endpoints;

public static class DeleteAccount
{
    public static IEndpointRouteBuilder MapDeleteAccount(this IEndpointRouteBuilder app)
    {
        // Shared blocks stay as an anonymous spam signal unless the user ticks the option to remove them.
        app.MapDelete(RegisterAccount.Route, async (bool? removeSharedBlocks, ISender sender, CancellationToken cancellationToken) =>
            {
                await sender.Send(new DeleteAccountCommand(removeSharedBlocks ?? false), cancellationToken);

                return TypedResults.NoContent();
            })
            .WithName(nameof(DeleteAccount));

        return app;
    }
}
