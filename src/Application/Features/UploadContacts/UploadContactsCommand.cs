using MediatR;

namespace Tranqui.Application.Features.UploadContacts;

/// <summary>One page of the user's address book, exactly as stored on the device.</summary>
public sealed record UploadContactsCommand(IReadOnlyList<ContactEntry> Contacts) : IRequest<UploadContactsResult>;

public sealed record ContactEntry(string PhoneNumber, string? Name);
