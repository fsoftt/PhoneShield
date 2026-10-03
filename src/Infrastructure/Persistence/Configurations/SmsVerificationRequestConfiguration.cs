using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranqui.Domain.Appeals;

namespace Tranqui.Infrastructure.Persistence.Configurations;

internal sealed class SmsVerificationRequestConfiguration : IEntityTypeConfiguration<SmsVerificationRequest>
{
    public void Configure(EntityTypeBuilder<SmsVerificationRequest> builder)
    {
        builder.HasKey(request => request.Id);
        builder.Property(request => request.Id).ValueGeneratedNever();
        builder.HasIndex(request => new { request.PhoneHash, request.RequestedAt });
    }
}
