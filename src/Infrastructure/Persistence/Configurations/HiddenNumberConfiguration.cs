using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranqui.Domain.Appeals;

namespace Tranqui.Infrastructure.Persistence.Configurations;

internal sealed class HiddenNumberConfiguration : IEntityTypeConfiguration<HiddenNumber>
{
    public void Configure(EntityTypeBuilder<HiddenNumber> builder) => builder.HasKey(hidden => hidden.PhoneHash);
}
