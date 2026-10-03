namespace Tranqui.Domain.Reputation;

public static class ContactUploadRules
{
    /// <summary>Uploads are split in batches so a request stays small; the app sends the address book in pages.</summary>
    public const int MaxContactsPerBatch = 500;

    /// <summary>Caps how much one account can weigh in the community data.</summary>
    public const int MaxContactsPerAccount = 5000;
}
