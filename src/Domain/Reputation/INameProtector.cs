using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Domain.Reputation;

/// <summary>Encrypts caller names so they can only be read by someone who already knows the phone number.</summary>
public interface INameProtector
{
    ProtectedName Protect(PhoneNumber phoneNumber, CallerName name);

    string Unprotect(PhoneNumber phoneNumber, ProtectedName protectedName);
}
