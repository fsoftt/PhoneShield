using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranqui.Domain.Reputation;

namespace Tranqui.Infrastructure.Persistence.Configurations;

internal sealed class ContributorReputationConfiguration : IEntityTypeConfiguration<ContributorReputation>
{
    public void Configure(EntityTypeBuilder<ContributorReputation> builder) => builder.HasKey(reputation => reputation.ContributorId);
}
