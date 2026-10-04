using FluentValidation;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Features.UnblockNumber;

internal sealed class UnblockNumberValidator : AbstractValidator<UnblockNumberCommand>
{
    public UnblockNumberValidator()
    {
        RuleFor(command => command.PhoneNumber)
            .Must(raw => PhoneNumber.TryParse(raw) is not null)
            .WithMessage("The phone number is not valid.");
    }
}
