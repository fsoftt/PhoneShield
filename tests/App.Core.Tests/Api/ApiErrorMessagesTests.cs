using System.Net;
using Refit;
using Tranqui.App.Core.Api;
using Tranqui.App.Core.Resources;
using Tranqui.Contracts.Errors;

namespace Tranqui.App.Core.Tests.Api;

public sealed class ApiErrorMessagesTests
{
    public static TheoryData<string, string> CodeMessages => new()
    {
        { ApiErrorCodes.ValidationFailed, Texts.ErrorInvalidRequest },
        { ApiErrorCodes.AccountNotRegistered, Texts.ErrorAccountNotRegistered },
        { ApiErrorCodes.ConsentRequired, Texts.ErrorConsentRequired },
        { ApiErrorCodes.RateLimited, Texts.ErrorTooManyRequests },
        { ApiErrorCodes.AppealLimitReached, Texts.ErrorAppealLimitReached },
        { ApiErrorCodes.AppealAccountTooNew, Texts.ErrorAppealAccountTooNew },
        { ApiErrorCodes.DeviceNotTrusted, Texts.ErrorDeviceNotTrusted },
        { ApiErrorCodes.PhoneNotVerified, Texts.ErrorPhoneNotVerified },
        { ApiErrorCodes.UnexpectedError, Texts.ErrorUnexpected },
    };

    [Theory]
    [MemberData(nameof(CodeMessages))]
    public async Task For_KnownCode_UsesTheLocalizedMessage(string code, string expected)
    {
        var exception = await CreateAsync(HttpStatusCode.Conflict, $$"""{"title":"English title","code":"{{code}}"}""");

        ApiErrorMessages.For(exception).Should().Be(expected);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.BadGateway)]
    public async Task For_NoCode_FallsBackToTheStatus(HttpStatusCode status)
    {
        var exception = await CreateAsync(status, "<html>proxy error</html>");

        ApiErrorMessages.For(exception).Should().Be(
            status == HttpStatusCode.TooManyRequests ? Texts.ErrorTooManyRequests : Texts.ErrorUnexpected);
    }

    [Fact]
    public async Task For_NeverShowsTheEnglishTitleFromTheApi()
    {
        var exception = await CreateAsync(HttpStatusCode.Conflict, """{"title":"The account must be registered first.","code":"account_not_registered"}""");

        ApiErrorMessages.For(exception).Should().NotContain("must be registered");
    }

    private static Task<ApiException> CreateAsync(HttpStatusCode status, string body) =>
        ApiException.Create(
            new HttpRequestMessage(HttpMethod.Post, new Uri("https://api.test/v1/reports")),
            HttpMethod.Post,
            new HttpResponseMessage(status) { Content = new StringContent(body) },
            new RefitSettings());
}
