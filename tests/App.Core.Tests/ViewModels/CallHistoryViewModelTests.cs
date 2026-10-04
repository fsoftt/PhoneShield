using NSubstitute;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Dialogs;
using Tranqui.App.Core.History;
using Tranqui.App.Core.Reports;
using Tranqui.App.Core.Resources;
using Tranqui.App.Core.Sync;
using Tranqui.App.Core.Tests.Sync;
using Tranqui.App.Core.ViewModels;
using Tranqui.Contracts.Reports;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class CallHistoryViewModelTests : IDisposable
{
    private const string Number = "+573001234567";

    private static readonly CallRecord unknownCall =
        new(Guid.NewGuid(), Number, Number, Number, CallerCardState.Unknown, null, DateTimeOffset.UtcNow);

    private readonly ICallHistory history = Substitute.For<ICallHistory>();
    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly IBlockList blockList = Substitute.For<IBlockList>();
    private readonly IDialogService dialogs = Substitute.For<IDialogService>();
    private readonly IScreeningSettingsStore settings = Substitute.For<IScreeningSettingsStore>();
    private readonly InMemoryMyReports myReports = new();
    private readonly Outbox outbox;
    private readonly CallHistoryViewModel viewModel;

    public CallHistoryViewModelTests()
    {
        history.ListAsync().Returns([unknownCall]);
        settings.Load().Returns(ScreeningSettings.Default);
        outbox = new Outbox(new InMemoryOutboxStore(), api, Substitute.For<IBackgroundSync>(), TimeProvider.System);
        viewModel = new CallHistoryViewModel(
            history,
            new ReportService(myReports, outbox, TimeProvider.System),
            new BlockingService(blockList, settings, outbox),
            dialogs);
    }

    public void Dispose() => outbox.Dispose();

    [Fact]
    public async Task ReportSpam_WithLabel_SendsItAndMarksTheEntry()
    {
        await viewModel.LoadCommand.ExecuteAsync(null);
        GivenPromptAnswer(" Spam Claro ");
        var entry = viewModel.Entries[0];

        await viewModel.ReportSpamCommand.ExecuteAsync(entry);
        await outbox.FlushAsync(CancellationToken.None);

        await api.Received(1).ReportCallAsync(new ReportCallRequest(Number, ReportVerdictDto.Spam, "Spam Claro"), Arg.Any<CancellationToken>());
        myReports.List().Should().ContainSingle(report => report.E164 == Number && report.Label == "Spam Claro");
        await history.Received(1).SetVerdictAsync(unknownCall.Id, ReportVerdictDto.Spam);
        entry.CanReport.Should().BeFalse();
        entry.Summary.Should().Be(Texts.Format(Texts.HistoryReportedSpamFormat, Texts.HistoryUnknown));
        viewModel.InfoMessage.Should().Be(Texts.ReportQueued);
    }

    [Fact]
    public async Task ReportSpam_WithoutLabel_SendsNoLabel()
    {
        await viewModel.LoadCommand.ExecuteAsync(null);
        GivenPromptAnswer(string.Empty);

        await viewModel.ReportSpamCommand.ExecuteAsync(viewModel.Entries[0]);
        await outbox.FlushAsync(CancellationToken.None);

        await api.Received(1).ReportCallAsync(new ReportCallRequest(Number, ReportVerdictDto.Spam, null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReportSpam_Cancelled_SendsNothing()
    {
        await viewModel.LoadCommand.ExecuteAsync(null);
        GivenPromptAnswer(null);

        await viewModel.ReportSpamCommand.ExecuteAsync(viewModel.Entries[0]);

        await api.DidNotReceive().ReportCallAsync(Arg.Any<ReportCallRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Block_AddsTheNumberToTheBlockList()
    {
        await viewModel.LoadCommand.ExecuteAsync(null);

        await viewModel.BlockCommand.ExecuteAsync(viewModel.Entries[0]);

        await blockList.Received(1).AddAsync(Arg.Is<PhoneNumber>(number => number.E164 == Number));
        await outbox.FlushAsync(CancellationToken.None);
        await api.Received(1).BlockAsync(new Contracts.Blocks.BlockRequest(Number), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Entry_FromKnownContact_CannotBeReported()
    {
        var entry = new CallHistoryEntry(unknownCall with { State = CallerCardState.KnownContact });

        entry.CanReport.Should().BeFalse();
    }

    [Fact]
    public void Entry_Blocked_SummarizesTheReason()
    {
        var entry = new CallHistoryEntry(unknownCall with { BlockReason = BlockReason.CommunitySpam });

        entry.Summary.Should().Be(Texts.Format(Texts.HistoryBlockedFormat, Texts.BlockReasonCommunitySpam));
    }

    private void GivenPromptAnswer(string? answer) =>
        dialogs.PromptAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>()).Returns(answer);
}
