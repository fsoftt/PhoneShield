using Tranqui.App.Core.Reports;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Storage;

internal sealed class SqliteMyReports(LocalDatabase database) : IMyReports
{
    public IReadOnlyList<MyReport> List()
    {
        using var connection = database.Open();
        using var query = connection.Command("SELECT e164, verdict, label, reported_at FROM my_reports ORDER BY reported_at DESC");
        using var reader = query.ExecuteReader();

        var reports = new List<MyReport>();
        while (reader.Read())
        {
            reports.Add(new MyReport(
                reader.GetString(0), (ReportVerdictDto)reader.GetInt32(1), reader.NullableString(2), reader.GetInt64(3).ToDateTimeOffset()));
        }

        return reports;
    }

    public void Save(MyReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        using var connection = database.Open();
        using var command = connection.Command(
            "INSERT OR REPLACE INTO my_reports (e164, verdict, label, reported_at) VALUES ($e164, $verdict, $label, $reported)",
            ("$e164", report.E164),
            ("$verdict", (int)report.Verdict),
            ("$label", report.Label),
            ("$reported", report.ReportedAt.ToStored()));
        command.ExecuteNonQuery();
    }

    public void Remove(string e164)
    {
        using var connection = database.Open();
        using var command = connection.Command("DELETE FROM my_reports WHERE e164 = $e164", ("$e164", e164));
        command.ExecuteNonQuery();
    }
}
