using Tranqui.App.Core.Reports;

namespace Tranqui.App.Core.Tests.Sync;

internal sealed class InMemoryMyReports : IMyReports
{
    private readonly Dictionary<string, MyReport> reports = new(StringComparer.Ordinal);

    public IReadOnlyList<MyReport> List() => [.. reports.Values];

    public void Save(MyReport report) => reports[report.E164] = report;

    public void Remove(string e164) => reports.Remove(e164);
}
