using FluentValidation;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Features.WithdrawReport;

internal sealed class WithdrawReportValidator : AbstractValidator<WithdrawReportCommand>
{
    public WithdrawReportValidator()
    {
        RuleFor(command => command.PhoneNumber)
            .Must(raw => PhoneNumber.TryParse(raw) is not null)
            .WithMessage("The phone number is not valid.");
    }
}
