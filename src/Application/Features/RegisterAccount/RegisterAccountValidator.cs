using FluentValidation;
using Tranqui.Domain.Legal;

namespace Tranqui.Application.Features.RegisterAccount;

internal sealed class RegisterAccountValidator : AbstractValidator<RegisterAccountCommand>
{
    public RegisterAccountValidator()
    {
        RuleFor(command => command.AcceptedTermsVersion)
            .Equal(LegalDocuments.CurrentTermsVersion)
            .WithMessage("Debes aceptar la versión vigente de los términos y la política de tratamiento de datos.");
    }
}
