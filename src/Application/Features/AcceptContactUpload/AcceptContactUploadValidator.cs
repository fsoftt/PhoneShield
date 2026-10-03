using FluentValidation;
using Tranqui.Domain.Legal;

namespace Tranqui.Application.Features.AcceptContactUpload;

internal sealed class AcceptContactUploadValidator : AbstractValidator<AcceptContactUploadCommand>
{
    public AcceptContactUploadValidator()
    {
        RuleFor(command => command.AcceptedVersion)
            .Equal(LegalDocuments.CurrentContactUploadVersion)
            .WithMessage("Debes aceptar la versión vigente del permiso para aportar contactos.");
    }
}
