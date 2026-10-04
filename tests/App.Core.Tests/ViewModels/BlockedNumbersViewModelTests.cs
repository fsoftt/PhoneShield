using NSubstitute;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.Sync;
using Tranqui.App.Core.Tests.Sync;
using Tranqui.App.Core.ViewModels;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class BlockedNumbersViewModelTests : IDisposable
{
    private static readonly PhoneNumber first = PhoneNumber.TryParse("+573001112233")!;
    private static readonly PhoneNumber second = PhoneNumber.TryParse("+573004445566")!;

    private readonly IBlockList blockList = Substitute.For<IBlockList>();
    private readonly IScreeningSettingsStore settings = Substitute.For<IScreeningSettingsStore>();
    private readonly ITranquiApi api = Substitute.For<ITranquiApi>();
    private readonly InMemoryOutboxStore outboxStore = new();
    private readonly Outbox outbox;
    private readonly BlockedNumbersViewModel viewModel;

    public BlockedNumbersViewModelTests()
    {
        settings.Load().Returns(ScreeningSettings.Default);
        api.UnblockAsync(Arg.Any<Contracts.Blocks.BlockRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromException(new HttpRequestException()));
        outbox = new Outbox(outboxStore, api, Substitute.For<IBackgroundSync>(), TimeProvider.System);
        viewModel = new BlockedNumbersViewModel(blockList, new BlockingService(blockList, settings, outbox));
    }

    public void Dispose() => outbox.Dispose();

    [Fact]
    public async Task Load_ListsTheBlockedNumbersInOrder()
    {
        blockList.ListAsync().Returns([second, first]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.Numbers.Should().Equal(first, second);
        viewModel.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task Unblock_RemovesFromTheListAndTheDevice()
    {
        blockList.ListAsync().Returns([first]);
        await viewModel.LoadCommand.ExecuteAsync(null);

        await viewModel.UnblockCommand.ExecuteAsync(first);

        await blockList.Received(1).RemoveAsync(first);
        outboxStore.Load().Should().ContainSingle(operation => operation.Kind == PendingOperationKind.Unblock && operation.E164 == first.E164);
        viewModel.Numbers.Should().BeEmpty();
        viewModel.IsEmpty.Should().BeTrue();
    }
}
