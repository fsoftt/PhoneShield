using System.Net.Http.Json;
using System.Text.Json;
using Tranqui.Contracts.Errors;

namespace Tranqui.Api.Tests;

public static class ProblemCodeAssertions
{
    /// <summary>The stable error code clients localize; the English title is for developers only.</summary>
    public static async Task<string?> ProblemCodeAsync(this HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

        return problem.TryGetProperty(ApiErrorCodes.Field, out var code) ? code.GetString() : null;
    }
}
