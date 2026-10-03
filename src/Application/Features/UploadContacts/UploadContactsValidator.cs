using FluentValidation;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.UploadContacts;

internal sealed class UploadContactsValidator : AbstractValidator<UploadContactsCommand>
{
    public UploadContactsValidator()
    {
        RuleFor(command => command.Contacts)
            .NotEmpty()
            .Must(contacts => contacts.Count <= ContactUploadRules.MaxContactsPerBatch)
            .WithMessage($"Envía como máximo {ContactUploadRules.MaxContactsPerBatch} contactos por lote.");
    }
}
