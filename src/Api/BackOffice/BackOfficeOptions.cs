namespace Tranqui.Api.BackOffice;

public sealed class BackOfficeOptions
{
    public const string SectionName = "BackOffice";

    /// <summary>Firebase uids allowed into the back office. Empty: nobody (the default).</summary>
    public IReadOnlyList<string> AdminUids { get; set; } = [];

    /// <summary>Run the daily maintenance: expired-data purge and reporter reputation.</summary>
    public bool AutomaticPurge { get; set; } = true;
}
