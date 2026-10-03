using FluentValidation;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.ReportCall;

internal sealed class ReportCallValidator : AbstractValidator<ReportCallCommand>
{
    public ReportCallValidator()
    {
        RuleFor(command => command.PhoneNumber)
            .Must(raw => PhoneNumber.TryParse(raw) is not null)
            .WithMessage("The phone number is not valid.");

        RuleFor(command => command.Verdict).IsInEnum();

        RuleFor(command => command.Label)
            .Must(label => CallerName.TryCreate(label) is not null)
            .When(command => !string.IsNullOrWhiteSpace(command.Label))
            .WithMessage($"The label can have at most {CallerName.MaxLength} characters.");

        RuleFor(command => command.Label)
            .Empty()
            .When(command => command.Verdict == ReportVerdict.NotSpam)
            .WithMessage("Only spam reports can have a label.");
    }
}
