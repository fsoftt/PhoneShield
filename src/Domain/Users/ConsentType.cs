namespace Tranqui.Domain.Users;

public enum ConsentType
{
    /// <summary>Terms and conditions plus the data processing policy, accepted at registration.</summary>
    TermsAndPrivacyPolicy = 1,

    /// <summary>Optional: contribute the address book (hashed numbers and encrypted names). Revocable at any time.</summary>
    ContactUpload = 2,
}
