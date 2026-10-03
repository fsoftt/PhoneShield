using CommunityToolkit.Mvvm.ComponentModel;

namespace PhoneShield.App.ViewModels;

public sealed partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = AppResources.HomeTitle;

    [ObservableProperty]
    public partial string Subtitle { get; set; } = AppResources.HomeSubtitle;
}
