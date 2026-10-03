using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranqui.Domain.Reputation;

namespace Tranqui.Infrastructure.Persistence.Configurations;

internal sealed class SpamReportConfiguration : IEntityTypeConfiguration<SpamReport>
{
    private const int VerdictMaxLength = 16;

    public void Configure(EntityTypeBuilder<SpamReport> builder)
    {
        builder.HasKey(report => report.Id);
        builder.Property(report => report.Id).ValueGeneratedNever();
        builder.Property(report => report.Verdict).HasConversion<string>().HasMaxLength(VerdictMaxLength);
        builder.Ignore(report => report.Label);

        builder.HasIndex(report => new { report.PhoneHash, report.ContributorId }).IsUnique();
    }
}
