using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpCenter.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    // The SQL Server-specific "datetime2(3)" column type is only applied when that provider
    // is used. Other providers such as SQLite reject this literal type string during DDL
    // ("near 'max': syntax error" — see the same pattern in AuditLogConfiguration) — behavior
    // in production is unchanged; this merely allows the relational tests to run
    // EnsureCreated() with another provider.
    private readonly bool _useSqlServerColumnTypes;

    public RefreshTokenConfiguration() : this(useSqlServerColumnTypes: true)
    {
    }

    public RefreshTokenConfiguration(bool useSqlServerColumnTypes)
    {
        _useSqlServerColumnTypes = useSqlServerColumnTypes;
    }

    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(128);
        builder.Property(x => x.RevokedReason).HasMaxLength(200);
        builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(128);
        builder.Property(x => x.CreatedByIp).HasMaxLength(64);
        builder.Property(x => x.RevokedByIp).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);

        if (_useSqlServerColumnTypes)
        {
            builder.Property(x => x.CreatedAt).HasColumnType("datetime2(3)");
            builder.Property(x => x.ExpiresAt).HasColumnType("datetime2(3)");
            builder.Property(x => x.RevokedAt).HasColumnType("datetime2(3)");
        }

        // The same token hash is unique per user.
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.ExpiresAt });
        builder.HasIndex(x => new { x.CustomerId, x.ExpiresAt });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            // Customer -> Account is already a cascading one-to-one relationship.
            // Keeping this FK non-cascading avoids SQL Server's multiple-cascade-path
            // error while preserving the token row until it is explicitly revoked/removed.
            .OnDelete(DeleteBehavior.Restrict);
    }
}
