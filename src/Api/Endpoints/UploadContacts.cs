using MediatR;
using Tranqui.Api.RateLimiting;
using Tranqui.Application.Features.UploadContacts;
using Tranqui.Contracts.Contacts;

namespace Tranqui.Api.Endpoints;

public static class UploadContacts
{
    public const string Route = "/v1/contacts";

    public static IEndpointRouteBuilder MapUploadContacts(this IEndpointRouteBuilder app)
    {
        app.MapPost(Route, async (UploadContactsRequest request, ISender sender, CancellationToken cancellationToken) =>
            {
                var contacts = request.Contacts.Select(contact => new ContactEntry(contact.PhoneNumber, contact.Name)).ToList();
                var result = await sender.Send(new UploadContactsCommand(contacts), cancellationToken);

                return TypedResults.Ok(new UploadContactsResponse(result.Accepted, result.Skipped));
            })
            .RequireRateLimiting(RateLimitingExtensions.ContactUploadPolicy)
            .WithName(nameof(UploadContacts));

        return app;
    }
}
