namespace Tranqui.Api.RateLimiting;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public LookupLimits Lookup { get; set; } = new();

    public ReportLimits Reports { get; set; } = new();

    public ContactUploadLimits ContactUploads { get; set; } = new();

    public sealed class LookupLimits
    {
        private const int DefaultPerHour = 60;
        private const int DefaultPerDay = 300;

        public int PerHour { get; set; } = DefaultPerHour;

        public int PerDay { get; set; } = DefaultPerDay;
    }

    public sealed class ReportLimits
    {
        private const int DefaultPerDay = 20;

        public int PerDay { get; set; } = DefaultPerDay;
    }

    public sealed class ContactUploadLimits
    {
        /// <summary>A full address book (5 000 contacts) takes 10 batches of 500; this leaves room for later syncs.</summary>
        private const int DefaultBatchesPerDay = 20;

        public int BatchesPerDay { get; set; } = DefaultBatchesPerDay;
    }
}
