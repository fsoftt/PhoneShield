using FluentValidation;
using Tranqui.Domain.Appeals;

namespace Tranqui.Application.Features.SubmitAppeal;

internal sealed class SubmitAppealValidator : AbstractValidator<SubmitAppealCommand>
{
    public SubmitAppealValidator()
    {
        RuleFor(command => command.PhoneProof)
            .NotEmpty()
            .MaximumLength(AppealRules.PhoneProofMaxLength);

        RuleFor(command => command.DeviceId)
            .Must(raw => DeviceId.TryParse(raw) is not null)
            .WithMessage("The device id is not valid.");

        RuleFor(command => command.IntegrityToken)
            .NotEmpty()
            .MaximumLength(AppealRules.IntegrityTokenMaxLength);

        RuleFor(command => command.Kind).IsInEnum();

        RuleFor(command => command.Reason)
            .MaximumLength(AppealRules.ReasonMaxLength);

        RuleFor(command => command.ContactEmail)
            .MaximumLength(AppealRules.ContactEmailMaxLength)
            .EmailAddress()
            .When(command => !string.IsNullOrWhiteSpace(command.ContactEmail));
    }
}
