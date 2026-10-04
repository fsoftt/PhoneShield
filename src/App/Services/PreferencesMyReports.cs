using Tranqui.App.Core.Reports;

namespace Tranqui.App.Services;

/// <summary>The user's own reports in the app's private storage.</summary>
internal sealed class PreferencesMyReports : IMyReports
{
    private readonly JsonPreferences<List<MyReport>> storage = new("tranqui.my_reports", () => []);

    public IReadOnlyList<MyReport> List() => storage.Read();

    public void Save(MyReport report) =>
        storage.Update(reports => [.. reports.Where(existing => existing.E164 != report.E164), report]);

    public void Remove(string e164) => storage.Update(reports => [.. reports.Where(existing => existing.E164 != e164)]);
}
