using MediatR;
using Tranqui.Application.Features.WithdrawContacts;
using Tranqui.Contracts.Contacts;

namespace Tranqui.Api.Endpoints;

public static class WithdrawContacts
{
    public static IEndpointRouteBuilder MapWithdrawContacts(this IEndpointRouteBuilder app)
    {
        app.MapDelete(UploadContacts.Route, async (ISender sender, CancellationToken cancellationToken) =>
            {
                var removed = await sender.Send(new WithdrawContactsCommand(), cancellationToken);

                return TypedResults.Ok(new WithdrawContactsResponse(removed));
            })
            .WithName(nameof(WithdrawContacts));

        return app;
    }
}
