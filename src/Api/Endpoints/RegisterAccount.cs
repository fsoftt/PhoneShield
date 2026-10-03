using MediatR;
using Tranqui.Application.Features.RegisterAccount;
using Tranqui.Contracts.Accounts;

namespace Tranqui.Api.Endpoints;

public static class RegisterAccount
{
    public const string Route = "/v1/account";

    public static IEndpointRouteBuilder MapRegisterAccount(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, async (RegisterAccountRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(new RegisterAccountCommand(request.AcceptedTermsVersion), cancellationToken);

                return TypedResults.Ok(new AccountResponse(result.Id, result.CreatedAt, result.AcceptedTermsVersion));
            })
            .WithName(nameof(RegisterAccount));

        return app;
    }
}
