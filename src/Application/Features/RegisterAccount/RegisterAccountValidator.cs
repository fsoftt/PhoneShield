using FluentValidation;
using Tranqui.Domain.Legal;

namespace Tranqui.Application.Features.RegisterAccount;

internal sealed class RegisterAccountValidator : AbstractValidator<RegisterAccountCommand>
{
    public RegisterAccountValidator()
    {
        RuleFor(command => command.AcceptedTermsVersion)
            .Equal(LegalDocuments.CurrentTermsVersion)
            .WithMessage("The current terms version must be accepted.");
    }
}
