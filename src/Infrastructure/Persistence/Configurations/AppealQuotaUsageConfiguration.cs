using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranqui.Domain.Appeals;

namespace Tranqui.Infrastructure.Persistence.Configurations;

internal sealed class AppealQuotaUsageConfiguration : IEntityTypeConfiguration<AppealQuotaUsage>
{
    private const int EnumMaxLength = 16;

    public void Configure(EntityTypeBuilder<AppealQuotaUsage> builder)
    {
        builder.HasKey(usage => usage.Id);
        builder.Property(usage => usage.Id).ValueGeneratedNever();
        builder.Property(usage => usage.SubjectKind).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(usage => usage.Action).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.HasIndex(usage => new { usage.SubjectKind, usage.SubjectKey, usage.Action, usage.UsedAt });
    }
}
