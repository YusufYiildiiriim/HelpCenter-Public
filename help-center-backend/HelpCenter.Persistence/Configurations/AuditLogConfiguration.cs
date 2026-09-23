using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpCenter.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    // SQL Server-specific column types ("nvarchar(max)", "datetime2(3)") are only applied
    // when that provider is used. Other providers such as SQLite reject these literal type
    // strings during schema creation. Production behavior is unchanged.
    // this merely allows the relational tests to run EnsureCreated() with another provider
    // such as SQLite.
    private readonly bool _useSqlServerColumnTypes;

    public AuditLogConfiguration() : this(useSqlServerColumnTypes: true)
    {
    }

    public AuditLogConfiguration(bool useSqlServerColumnTypes)
    {
        _useSqlServerColumnTypes = useSqlServerColumnTypes;
    }

    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ActorEmail).HasMaxLength(256);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.CorrelationId).HasMaxLength(128);
        builder.Property(x => x.TargetType).HasMaxLength(100);

        if (_useSqlServerColumnTypes)
        {
            builder.Property(x => x.BeforeJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.AfterJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.MetadataJson).HasColumnType("nvarchar(max)");
            builder.Property(x => x.OccurredAt).HasColumnType("datetime2(3)");
        }

        builder.Property(x => x.OccurredAt).IsRequired();

        builder.HasIndex(x => new { x.EventType, x.OccurredAt });
        builder.HasIndex(x => new { x.ActorUserId, x.OccurredAt });
        builder.HasIndex(x => new { x.TargetType, x.TargetId });
    }
}
