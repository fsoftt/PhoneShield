namespace Tranqui.App.Core.Reports;

/// <summary>The user's own reports, stored only on the phone (the server cannot list them: it only has hashes).</summary>
public interface IMyReports
{
    IReadOnlyList<MyReport> List();

    /// <summary>Adds or replaces the report for the number.</summary>
    void Save(MyReport report);

    void Remove(string e164);
}
