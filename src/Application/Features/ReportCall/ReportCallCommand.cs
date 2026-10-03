using MediatR;
using Tranqui.Domain.Reputation;

namespace Tranqui.Application.Features.ReportCall;

/// <summary>After a call, the user says whether it was spam, optionally labelling it (e.g. "Spam Claro").</summary>
public sealed record ReportCallCommand(string PhoneNumber, ReportVerdict Verdict, string? Label) : IRequest;
