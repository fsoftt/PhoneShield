using FluentValidation;

namespace Tranqui.Application.Features.ResolveAppeal;

internal sealed class ResolveAppealValidator : AbstractValidator<ResolveAppealCommand>
{
    public ResolveAppealValidator()
    {
        RuleFor(command => command.AppealId).NotEmpty();
        RuleFor(command => command.Decision).IsInEnum();
    }
}
