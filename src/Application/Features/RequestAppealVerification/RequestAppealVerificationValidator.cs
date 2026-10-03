using FluentValidation;
using Tranqui.Domain.PhoneNumbers;

namespace Tranqui.Application.Features.RequestAppealVerification;

internal sealed class RequestAppealVerificationValidator : AbstractValidator<RequestAppealVerificationCommand>
{
    public RequestAppealVerificationValidator()
    {
        RuleFor(command => command.PhoneNumber)
            .Must(raw => PhoneNumber.TryParse(raw) is not null)
            .WithMessage("The phone number is not valid.");
    }
}
