using FluentValidation;
using Tranqui.Domain.PhoneNumbers;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.ReportCall;

internal sealed class ReportCallValidator : AbstractValidator<ReportCallCommand>
{
    public ReportCallValidator()
    {
        RuleFor(command => command.PhoneNumber)
            .Must(raw => PhoneNumber.TryParse(raw) is not null)
            .WithMessage("El número de teléfono no es válido.");

        RuleFor(command => command.Verdict).IsInEnum();

        RuleFor(command => command.Label)
            .Must(label => CallerName.TryCreate(label) is not null)
            .When(command => !string.IsNullOrWhiteSpace(command.Label))
            .WithMessage($"La etiqueta debe tener máximo {CallerName.MaxLength} caracteres.");

        RuleFor(command => command.Label)
            .Empty()
            .When(command => command.Verdict == ReportVerdict.NotSpam)
            .WithMessage("Solo los reportes de spam pueden llevar etiqueta.");
    }
}
