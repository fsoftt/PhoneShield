using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranqui.Domain.Reputation;

namespace Tranqui.Infrastructure.Persistence.Configurations;

internal sealed class ContactContributionConfiguration : IEntityTypeConfiguration<ContactContribution>
{
    public void Configure(EntityTypeBuilder<ContactContribution> builder)
    {
        builder.HasKey(contribution => contribution.Id);
        builder.Property(contribution => contribution.Id).ValueGeneratedNever();
        builder.Ignore(contribution => contribution.Name);

        builder.HasIndex(contribution => new { contribution.PhoneHash, contribution.ContributorId }).IsUnique();
    }
}
