using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranqui.Domain.Reputation;

namespace Tranqui.Infrastructure.Persistence.Configurations;

internal sealed class BlockSignalConfiguration : IEntityTypeConfiguration<BlockSignal>
{
    public void Configure(EntityTypeBuilder<BlockSignal> builder)
    {
        builder.HasKey(block => new { block.PhoneHash, block.ContributorId });
        builder.HasIndex(block => block.ContributorId);
    }
}
