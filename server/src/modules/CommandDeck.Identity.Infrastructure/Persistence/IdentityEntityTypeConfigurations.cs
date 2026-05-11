using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CommandDeck.Identity.Access;
using CommandDeck.Identity.Users;

namespace CommandDeck.Identity.Infrastructure.Persistence;

public sealed class LocalUserConfiguration : IEntityTypeConfiguration<LocalUser>
{
    public void Configure(EntityTypeBuilder<LocalUser> builder)
    {
        builder.ToTable("local_users", "identity");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Provider).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(256).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(256);
        builder.Property(x => x.Email)
            .HasConversion(
                email => email == null ? null : email.Value,
                value => EmailAddress.FromNullable(value))
            .HasColumnName("email")
            .HasMaxLength(320);
        builder.HasIndex(x => new { x.Provider, x.Subject }).IsUnique();
    }
}

public sealed class ApplicationAccessConfiguration
    : IEntityTypeConfiguration<ApplicationAccess>
{
    public void Configure(EntityTypeBuilder<ApplicationAccess> builder)
    {
        builder.ToTable("application_access", "identity");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LocalUserId).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.HasIndex(x => x.LocalUserId).IsUnique();
    }
}
