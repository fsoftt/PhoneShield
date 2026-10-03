using Tranqui.Application.Abstractions;

namespace Tranqui.Api.Authentication;

internal sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public string FirebaseUid =>
        httpContextAccessor.HttpContext?.User.FindFirst(FirebaseClaims.UserId)?.Value
        ?? throw new InvalidOperationException("No authenticated user in the current request.");
}
