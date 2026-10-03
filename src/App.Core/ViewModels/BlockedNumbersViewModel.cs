using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Calls;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.App.Core.ViewModels;

/// <summary>The user's own block list (stored only on the phone), with the full number since it never leaves the device.</summary>
public sealed partial class BlockedNumbersViewModel(IBlockList blockList) : ObservableObject
{
    public ObservableCollection<PhoneNumber> Numbers { get; } = [];

    [ObservableProperty]
    public partial bool IsEmpty { get; set; } = true;

    [RelayCommand]
    private async Task LoadAsync()
    {
        Numbers.Clear();
        foreach (var number in (await blockList.ListAsync()).OrderBy(number => number.E164, StringComparer.Ordinal))
        {
            Numbers.Add(number);
        }

        IsEmpty = Numbers.Count == 0;
    }

    [RelayCommand]
    private async Task UnblockAsync(PhoneNumber number)
    {
        await blockList.RemoveAsync(number);
        Numbers.Remove(number);
        IsEmpty = Numbers.Count == 0;
    }
}
