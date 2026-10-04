using NSubstitute;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Dialogs;
using Tranqui.App.Core.Reports;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.Sync;
using Tranqui.App.Core.Tests.Sync;
using Tranqui.App.Core.ViewModels;
using Tranqui.Contracts.Reports;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class MyReportsViewModelTests : IDisposable
{
    private const string Number = "+573001234567";

    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly IDialogService dialogs = Substitute.For<IDialogService>();
    private readonly InMemoryMyReports myReports = new();
    private readonly InMemoryOutboxStore outboxStore = new();
    private readonly Outbox outbox;
    private readonly MyReportsViewModel viewModel;

    public MyReportsViewModelTests()
    {
        // Offline: everything stays queued, which is what the screen shows as pending.
        api.ReportCallAsync(Arg.Any<ReportCallRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new HttpRequestException()));
        api.WithdrawReportAsync(Arg.Any<WithdrawReportRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new HttpRequestException()));
        outbox = new Outbox(outboxStore, api, Substitute.For<IBackgroundSync>(), TimeProvider.System);
        viewModel = new MyReportsViewModel(myReports, new ReportService(myReports, outbox, TimeProvider.System), outbox, dialogs);
    }

    public void Dispose() => outbox.Dispose();

    [Fact]
    public async Task Load_ShowsReportsAndWhichAreStillPending()
    {
        await new ReportService(myReports, outbox, TimeProvider.System).ReportAsync(Number, ReportVerdictDto.Spam, "Spam Claro");

        viewModel.LoadCommand.Execute(null);

        var entry = viewModel.Entries.Should().ContainSingle().Subject;
        entry.VerdictText.Should().Be(Texts.Format(Texts.MyReportSpamWithLabelFormat, "Spam Claro"));
        entry.IsPending.Should().BeTrue();
        viewModel.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task ChangeToNotSpam_ReplacesTheReport()
    {
        myReports.Save(new MyReport(Number, ReportVerdictDto.Spam, "Spam Claro", DateTimeOffset.UtcNow));
        viewModel.LoadCommand.Execute(null);

        await viewModel.ChangeToNotSpamCommand.ExecuteAsync(viewModel.Entries[0]);

        myReports.List().Should().ContainSingle(report => report.Verdict == ReportVerdictDto.NotSpam && report.Label == null);
        outboxStore.Load().Should().ContainSingle(operation => operation.Verdict == ReportVerdictDto.NotSpam);
    }

    [Fact]
    public async Task Withdraw_Confirmed_RemovesItAndQueuesTheWithdrawal()
    {
        myReports.Save(new MyReport(Number, ReportVerdictDto.Spam, null, DateTimeOffset.UtcNow));
        dialogs.ConfirmAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        viewModel.LoadCommand.Execute(null);

        await viewModel.WithdrawCommand.ExecuteAsync(viewModel.Entries[0]);

        myReports.List().Should().BeEmpty();
        outboxStore.Load().Should().ContainSingle(operation => operation.Kind == PendingOperationKind.WithdrawReport);
        viewModel.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task Withdraw_NotConfirmed_KeepsIt()
    {
        myReports.Save(new MyReport(Number, ReportVerdictDto.Spam, null, DateTimeOffset.UtcNow));
        viewModel.LoadCommand.Execute(null);

        await viewModel.WithdrawCommand.ExecuteAsync(viewModel.Entries[0]);

        myReports.List().Should().ContainSingle();
    }
}
