using MediatR;
using Tranqui.Domain.Appeals;

namespace Tranqui.Application.Features.ResolveAppeal;

/// <summary>Back office: a person approves or rejects a pending spam review.</summary>
public sealed record ResolveAppealCommand(Guid AppealId, AppealDecision Decision) : IRequest<AppealStatus>;
