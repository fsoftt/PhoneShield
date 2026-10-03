using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tranqui.App.Core.Appeals;
using Tranqui.App.Core.Resources;
using Tranqui.Contracts.Appeals;

namespace Tranqui.App.Core.ViewModels;

/// <summary>Appeal for the user's own number in two steps: ask for the SMS code, then send the appeal with it.</summary>
public sealed partial class AppealViewModel(AppealService appeals) : FormViewModel
{
    private const int CodeLength = 6;

    private AppealCodeRequest? codeRequest;

    [ObservableProperty]
    public partial string? PhoneNumber { get; set; }

    [ObservableProperty]
    public partial bool IsReviewSpam { get; set; }

    [ObservableProperty]
    public partial string? Reason { get; set; }

    [ObservableProperty]
    public partial string? ContactEmail { get; set; }

    [ObservableProperty]
    public partial string? Code { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDetailsStep), nameof(IsCodeStep), nameof(IsDone))]
    public partial AppealStep Step { get; set; }

    [ObservableProperty]
    public partial string? CodeSentMessage { get; set; }

    [ObservableProperty]
    public partial string? ResultMessage { get; set; }

    public bool IsDetailsStep => Step == AppealStep.Details;

    public bool IsCodeStep => Step == AppealStep.Code;

    public bool IsDone => Step == AppealStep.Done;

    [RelayCommand]
    private async Task SendCodeAsync()
    {
        var phoneNumber = Domain.PhoneNumbers.PhoneNumber.TryParse(PhoneNumber);
        if (!Require(phoneNumber is not null, Texts.InvalidPhoneNumber))
        {
            return;
        }

        await RunAsync(async () =>
        {
            codeRequest = await appeals.RequestCodeAsync(phoneNumber!, CancellationToken.None);
            CodeSentMessage = Texts.Format(Texts.AppealCodeSentFormat, codeRequest.PhoneNumber);
            Step = AppealStep.Code;
        });
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        var code = Code?.Trim() ?? string.Empty;
        if (codeRequest is null || !Require(code.Length == CodeLength && code.All(char.IsAsciiDigit), Texts.AppealCodeRequired))
        {
            return;
        }

        await RunAsync(async () =>
        {
            var status = await appeals.SubmitAsync(
                codeRequest,
                code,
                IsReviewSpam ? AppealKindDto.ReviewSpam : AppealKindDto.HideNames,
                string.IsNullOrWhiteSpace(Reason) ? null : Reason.Trim(),
                IsReviewSpam && !string.IsNullOrWhiteSpace(ContactEmail) ? ContactEmail.Trim() : null,
                CancellationToken.None);
            ResultMessage = status == AppealStatusDto.Applied ? Texts.AppealApplied : Texts.AppealPending;
            Step = AppealStep.Done;
        });
    }
}
