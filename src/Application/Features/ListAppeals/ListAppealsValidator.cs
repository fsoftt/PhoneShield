using FluentValidation;

namespace Tranqui.Application.Features.ListAppeals;

internal sealed class ListAppealsValidator : AbstractValidator<ListAppealsQuery>
{
    public ListAppealsValidator()
    {
        RuleFor(query => query.Status).IsInEnum();
    }
}
