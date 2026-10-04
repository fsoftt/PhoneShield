using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranqui.Domain.Appeals;

namespace Tranqui.Infrastructure.Persistence.Configurations;

internal sealed class AppealConfiguration : IEntityTypeConfiguration<Appeal>
{
    private const int EnumMaxLength = 16;

    public void Configure(EntityTypeBuilder<Appeal> builder)
    {
        builder.HasKey(appeal => appeal.Id);
        builder.Property(appeal => appeal.Id).ValueGeneratedNever();
        builder.Property(appeal => appeal.Kind).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(appeal => appeal.Status).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(appeal => appeal.Reason).HasMaxLength(AppealRules.ReasonMaxLength);
        builder.Property(appeal => appeal.ContactEmail).HasMaxLength(AppealRules.ContactEmailMaxLength);
        builder.Property(appeal => appeal.HashKeyVersion).HasDefaultValue(1);
        builder.Ignore(appeal => appeal.Hash);
        builder.Ignore(appeal => appeal.DueAt);
        builder.HasIndex(appeal => new { appeal.PhoneHash, appeal.CreatedAt });
        builder.HasIndex(appeal => new { appeal.Status, appeal.CreatedAt });
    }
}
