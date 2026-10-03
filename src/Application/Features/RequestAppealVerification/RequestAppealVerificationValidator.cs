using FluentValidation;
using Tranqui.Domain.Appeals;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Features.RequestAppealVerification;

internal sealed class RequestAppealVerificationValidator : AbstractValidator<RequestAppealVerificationCommand>
{
    public RequestAppealVerificationValidator()
    {
        RuleFor(command => command.PhoneNumber)
            .Must(raw => PhoneNumber.TryParse(raw) is not null)
            .WithMessage("The phone number is not valid.");

        RuleFor(command => command.DeviceId)
            .Must(raw => DeviceId.TryParse(raw) is not null)
            .WithMessage("The device id is not valid.");

        RuleFor(command => command.IntegrityToken)
            .NotEmpty()
            .MaximumLength(AppealRules.IntegrityTokenMaxLength);
    }
}
