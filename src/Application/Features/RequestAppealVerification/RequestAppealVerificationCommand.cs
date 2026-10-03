using MediatR;

namespace Tranqui.Application.Features.RequestAppealVerification;

/// <summary>
/// Asked before the website lets Firebase send a verification SMS, so each number gets at most one SMS per window.
/// Returns the normalized number the website must verify.
/// </summary>
public sealed record RequestAppealVerificationCommand(string PhoneNumber) : IRequest<string>;
