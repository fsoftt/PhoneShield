namespace Tranqui.Api.RateLimiting;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public LookupLimits Lookup { get; set; } = new();

    public sealed class LookupLimits
    {
        private const int DefaultPerHour = 60;
        private const int DefaultPerDay = 300;

        public int PerHour { get; set; } = DefaultPerHour;

        public int PerDay { get; set; } = DefaultPerDay;
    }
}
