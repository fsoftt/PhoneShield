namespace Tranqui.Api.BackOffice;

public sealed class BackOfficeOptions
{
    public const string SectionName = "BackOffice";

    /// <summary>Firebase uids allowed into the back office. Empty: nobody (the default).</summary>
    public IReadOnlyList<string> AdminUids { get; set; } = [];

    /// <summary>Firebase Web API key the back office page signs in with. Public, not a secret.</summary>
    public string FirebaseApiKey { get; set; } = string.Empty;

    /// <summary>Run the expired-data purge once a day.</summary>
    public bool AutomaticPurge { get; set; } = true;
}
