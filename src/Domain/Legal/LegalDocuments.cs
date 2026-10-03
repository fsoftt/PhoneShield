namespace Tranqui.Domain.Legal;

/// <summary>
/// Versions of the legal texts a user must accept. Bump a version whenever its text changes so every
/// consent can be traced to the exact wording the user saw (Ley 1581 de 2012).
/// </summary>
public static class LegalDocuments
{
    public const string CurrentTermsVersion = "2026-10-03";

    /// <summary>The separate, optional consent to contribute the address book.</summary>
    public const string CurrentContactUploadVersion = "2026-10-03";
}
