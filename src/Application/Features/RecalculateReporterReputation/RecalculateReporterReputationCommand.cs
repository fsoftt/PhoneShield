using MediatR;

namespace Tranqui.Application.Features.RecalculateReporterReputation;

/// <summary>Rates every reporter against the community's settled verdicts. Runs daily.</summary>
/// <returns>How many reporters got a multiplier other than 1.</returns>
public sealed record RecalculateReporterReputationCommand : IRequest<int>;
