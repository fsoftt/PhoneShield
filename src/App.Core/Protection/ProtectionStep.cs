using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tranqui.App.Core.Protection;

/// <summary>One permission in the setup checklist, with why it is needed in plain words.</summary>
public sealed partial class ProtectionStep(string title, string explanation, Func<Task> request) : ObservableObject
{
    public string Title { get; } = title;

    public string Explanation { get; } = explanation;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPending))]
    public partial bool IsDone { get; set; }

    public bool IsPending => !IsDone;

    [RelayCommand]
    private Task EnableAsync() => request();
}
