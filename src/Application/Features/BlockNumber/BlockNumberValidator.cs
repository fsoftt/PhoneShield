using FluentValidation;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Features.BlockNumber;

internal sealed class BlockNumberValidator : AbstractValidator<BlockNumberCommand>
{
    public BlockNumberValidator()
    {
        RuleFor(command => command.PhoneNumber)
            .Must(raw => PhoneNumber.TryParse(raw) is not null)
            .WithMessage("The phone number is not valid.");
    }
}
