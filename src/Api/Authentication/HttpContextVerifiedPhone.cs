using Tranqui.Application.Abstractions;

namespace Tranqui.Api.Authentication;

internal sealed class HttpContextVerifiedPhone(IHttpContextAccessor httpContextAccessor) : IVerifiedPhone
{
    public string E164 =>
        httpContextAccessor.HttpContext?.User.FindFirst(FirebaseClaims.PhoneNumber)?.Value
        ?? throw new InvalidOperationException("No SMS-verified phone number in the current request.");
}
