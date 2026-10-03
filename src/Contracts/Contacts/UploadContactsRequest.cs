namespace Tranqui.Contracts.Contacts;

public sealed record UploadContactsRequest(IReadOnlyList<ContactDto> Contacts);

public sealed record ContactDto(string PhoneNumber, string? Name);
