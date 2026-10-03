using NSubstitute;
using Tranqui.App.Core.Calls;
using Tranqui.App.Core.ViewModels;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.Tests.ViewModels;

public sealed class BlockedNumbersViewModelTests
{
    private static readonly PhoneNumber first = PhoneNumber.TryParse("+573001112233")!;
    private static readonly PhoneNumber second = PhoneNumber.TryParse("+573004445566")!;

    private readonly IBlockList blockList = Substitute.For<IBlockList>();
    private readonly BlockedNumbersViewModel viewModel;

    public BlockedNumbersViewModelTests()
    {
        viewModel = new BlockedNumbersViewModel(blockList);
    }

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
        viewModel.Numbers.Should().BeEmpty();
        viewModel.IsEmpty.Should().BeTrue();
    }
}
