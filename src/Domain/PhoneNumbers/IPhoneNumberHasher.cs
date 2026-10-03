namespace Tranqui.Domain.PhoneNumbers;

/// <summary>Produces the keyed hash of a phone number. The secret key never leaves the implementation.</summary>
public interface IPhoneNumberHasher
{
    PhoneHash Hash(PhoneNumber phoneNumber);
}
