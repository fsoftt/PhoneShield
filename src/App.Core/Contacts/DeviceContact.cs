namespace Tranqui.App.Core.Contacts;

/// <summary>One phone number from the address book, as stored on the device.</summary>
public sealed record DeviceContact(string PhoneNumber, string? Name);
