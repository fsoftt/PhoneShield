using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Calls;

/// <summary>The phone's own address book, read locally. Decides the green state without any network call.</summary>
public interface IDeviceContacts
{
    string? FindName(PhoneNumber number);
}
