using FluentValidation;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Features.LookupNumber;

internal sealed class LookupNumberValidator : AbstractValidator<LookupNumberQuery>
{
    public LookupNumberValidator()
    {
        RuleFor(query => query.PhoneNumber)
            .Must(raw => PhoneNumber.TryParse(raw) is not null)
            .WithMessage("El número de teléfono no es válido.");
    }
}
