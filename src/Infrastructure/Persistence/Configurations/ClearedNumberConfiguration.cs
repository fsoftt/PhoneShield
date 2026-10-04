using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranqui.Domain.Appeals;

namespace Tranqui.Infrastructure.Persistence.Configurations;

internal sealed class ClearedNumberConfiguration : IEntityTypeConfiguration<ClearedNumber>
{
    public void Configure(EntityTypeBuilder<ClearedNumber> builder) => builder.HasKey(cleared => cleared.PhoneHash);
}
