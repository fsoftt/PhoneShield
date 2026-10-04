using MediatR;

namespace Tranqui.Application.Features.WithdrawReport;

/// <summary>The user takes back their report on a number (from "My reports"). Idempotent.</summary>
public sealed record WithdrawReportCommand(string PhoneNumber) : IRequest;
