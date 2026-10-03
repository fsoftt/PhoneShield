using MediatR;
using Tranqui.Domain.Appeals;

namespace Tranqui.Application.Features.SubmitAppeal;

/// <summary>Filed by the SMS-verified owner of a number; which number comes from the verified token, never the body.</summary>
public sealed record SubmitAppealCommand(AppealKind Kind, string? Reason, string? ContactEmail) : IRequest<AppealStatus>;
