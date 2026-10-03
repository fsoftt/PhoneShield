using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tranqui.Domain.Users;

namespace Tranqui.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    private const int FirebaseUidMaxLength = 128;
    private const int ConsentVersionMaxLength = 32;
    private const int ConsentTypeMaxLength = 64;

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.FirebaseUid).HasMaxLength(FirebaseUidMaxLength);
        builder.HasIndex(user => user.FirebaseUid).IsUnique();

        builder.OwnsMany(user => user.Consents, consents =>
        {
            consents.ToTable("user_consents");
            consents.WithOwner().HasForeignKey("UserId");
            consents.Property<int>("Id");
            consents.HasKey("Id");
            consents.Property(consent => consent.Type).HasConversion<string>().HasMaxLength(ConsentTypeMaxLength);
            consents.Property(consent => consent.Version).HasMaxLength(ConsentVersionMaxLength);
        });

        builder.Navigation(user => user.Consents).HasField("consents");
    }
}
