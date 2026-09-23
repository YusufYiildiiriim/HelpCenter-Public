using System.Linq.Expressions;
using System.Reflection;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context.Seed;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Context;

public class EfContext : DbContext
{
    public EfContext(DbContextOptions<EfContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; } = default!;
    public DbSet<Account> Accounts { get; set; } = default!;
    public DbSet<Customer> Customers { get; set; } = default!;
    public DbSet<CustomerRequest> CustomerRequests { get; set; } = default!;
    public DbSet<CustomerRequestEvaluation> CustomerRequestEvaluations { get; set; } = default!;
    public DbSet<CustomerRequestMessage> CustomerRequestMessages { get; set; } = default!;
    public DbSet<CustomerRequestMessageDocument> CustomerRequestDocuments { get; set; } = default!;
    public DbSet<Conversation> Conversations { get; set; } = default!;
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; } = default!;
    public DbSet<RequestHistory> RequestHistories { get; set; } = default!;
    public DbSet<CustomerRequestStatus> CustomerRequestStatuses { get; set; } = default!;
    public DbSet<Document> Documents { get; set; } = default!;
    public DbSet<Domain.Entities.Module> Modules { get; set; } = default!;
    public DbSet<Guide> Guides { get; set; } = default!;
    public DbSet<FAQ> FAQs { get; set; } = default!;
    public DbSet<RequestSubject> RequestSubjects { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Role> Roles { get; set; } = default!;
    public DbSet<UserRole> UserRoles { get; set; } = default!;
    public DbSet<ModuleExpert> ModuleExperts { get; set; } = default!;
    public DbSet<RolePermission> RolePermissions { get; set; } = default!;
    public DbSet<RolePermissionAction> RolePermissionActions { get; set; } = default!;
    public DbSet<Project> Projects { get; set; } = default!;
    public DbSet<UserProject> UserProjects { get; set; } = default!;
    public DbSet<CompanyModule> CompanyModules { get; set; } = default!;
    public DbSet<ProjectModule> ProjectModules { get; set; } = default!;
    public DbSet<MenuItem> MenuItems { get; set; } = default!;
    public DbSet<AuditLog> AuditLogs { get; set; } = default!;
    public DbSet<OrganizationInfo> OrganizationInfos { get; set; } = default!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The SQL Server-specific column types in AuditLogConfiguration/RefreshTokenConfiguration
        // (nvarchar(max), datetime2(3)) are only applied for that provider — so those two are
        // excluded from assembly scanning and applied manually with the provider info. All other
        // configurations (those with a parameterless ctor) continue to be scanned unchanged.
        var isSqlServer = Database.IsSqlServer();
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly(), t =>
            t != typeof(Configurations.AuditLogConfiguration) && t != typeof(Configurations.RefreshTokenConfiguration));
        modelBuilder.ApplyConfiguration(new Configurations.AuditLogConfiguration(isSqlServer));
        modelBuilder.ApplyConfiguration(new Configurations.RefreshTokenConfiguration(isSqlServer));

        // Explicit table names: the CompanyModules/ProjectModules DbSet properties are named
        // in plural (for test ergonomics), but EF's default naming convention would then map
        // them to plural table names, while the migrations created them as singular
        // (CompanyModule/ProjectModule). Without this, every query touching either entity
        // throws "Invalid object name 'CompanyModules'/'ProjectModules'".
        modelBuilder.Entity<CompanyModule>().ToTable("CompanyModule");
        modelBuilder.Entity<ProjectModule>().ToTable("ProjectModule");

        // Global Query Filter (IsDeleted = false)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));

                // RowVersion Configuration
                var rowVersionProperty = modelBuilder.Entity(entityType.ClrType)
                    .Property("RowVersion")
                    .IsRowVersion()
                    .HasValueGenerator<HelpCenter.Persistence.ValueGeneration.RowVersionValueGenerator>();

                // Value-generated properties are automatically excluded from the INSERTs of
                // HasData() seed rows (EF Core assumes the value will be provided by the
                // store/generator). On SQL Server this worked fine thanks to the automatic value
                // generation inherited from the column's native rowversion type; on non-SQL
                // Server providers (e.g. SQLite in tests) the column is just a NOT NULL BLOB, so
                // these rows hit a constraint error. We only define a DEFAULT value OUTSIDE of
                // SQL Server — production (SQL Server) behavior is unchanged.
                if (!isSqlServer)
                {
                    rowVersionProperty.HasDefaultValue(new byte[8]);
                }

                // PublicId Configuration — a real/randomly generated, stored external identity
                // for each entity, replacing the XOR/HMAC scheme that "obfuscated" the int Id
                // reversibly with a fixed salt embedded in HashService's source code.
                // Same pattern as RowVersion: a ValueGenerator assigns a client-side Guid to
                // entities added via SaveChanges (including InMemory/SQLite tests); SQL
                // Server also has a NEWID() default — for both new rows and existing rows
                // that the ADD COLUMN migration will backfill.
                var publicIdProperty = modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.PublicId))
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<HelpCenter.Persistence.ValueGeneration.PublicIdValueGenerator>();

                if (isSqlServer)
                {
                    publicIdProperty.HasDefaultValueSql("NEWID()");
                }
                else
                {
                    // SQLite has no NEWID() — this is the standard SQLite expression that
                    // generates a random UUID string in RFC 4122 v4 format. Rows seeded with
                    // HasData skip SaveChanges (see the same comment on RowVersion), so these rows
                    // get their PublicId solely from this DB-side default; since it is evaluated
                    // per row, the values stay unique.
                    publicIdProperty.HasDefaultValueSql(
                        "(lower(hex(randomblob(4)) || '-' || hex(randomblob(2)) || '-4' || " +
                        "substr(hex(randomblob(2)),2) || '-' || substr('89ab', abs(random()) % 4 + 1, 1) || " +
                        "substr(hex(randomblob(2)),2) || '-' || hex(randomblob(6))))");
                }

                modelBuilder.Entity(entityType.ClrType).HasIndex(nameof(BaseEntity.PublicId)).IsUnique();

                modelBuilder.Entity(entityType.ClrType).Ignore("DomainEvents");
            }
        }

        modelBuilder.Entity<Customer>(b =>
        {
            b.Ignore(c => c.Email);
            b.Ignore(c => c.FirstName);
            b.Ignore(c => c.LastName);
            b.Ignore(c => c.Password);
            b.Ignore(c => c.IsPasswordChangeRequired);
            b.Ignore(c => c.PhoneNumber);
            b.Ignore(c => c.FullName);
        });

        modelBuilder.Entity<User>(b =>
        {
            b.Ignore(u => u.Email);
            b.Ignore(u => u.FirstName);
            b.Ignore(u => u.LastName);
            b.Ignore(u => u.Password);
            b.Ignore(u => u.IsPasswordChangeRequired);
            b.Ignore(u => u.Username);
            b.Ignore(u => u.FullName);
        });

        modelBuilder.Entity<FAQ>(b =>
        {
            b.HasOne(f => f.Project)
             .WithMany()
             .HasForeignKey(f => f.ProjectId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(f => f.Module)
             .WithMany()
             .HasForeignKey(f => f.ModuleId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Guide>()
            .HasOne(r => r.PreviousRequest)
            .WithOne(r => r.NextRequest)
            .HasForeignKey<Guide>(r => r.PreviousGuideId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        modelBuilder.Entity<RolePermissionAction>(b =>
        {
            b.HasOne(a => a.RolePermission)
             .WithMany(rp => rp.Actions)
             .HasForeignKey(a => a.RolePermissionId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(a => new { a.RolePermissionId, a.Action }).IsUnique();
        });

// Conversation Configurations
        modelBuilder.Entity<Conversation>()
            .HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Conversation>()
            .HasMany(c => c.Participants)
            .WithOne(p => p.Conversation)
            .HasForeignKey(p => p.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ConversationParticipant>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ConversationParticipant>()
            .HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerRequestMessage>()
            .HasOne(m => m.SenderParticipant)
            .WithMany(p => p.Messages)
            .HasForeignKey(m => m.SenderParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CustomerRequest>()
            .HasOne(r => r.Conversation)
            .WithMany()
            .HasForeignKey(r => r.ConversationId)
            .OnDelete(DeleteBehavior.SetNull);

        // OrganizationInfo Configuration - singleton entity
        modelBuilder.Entity<OrganizationInfo>(entity =>
        {
            entity.Property(e => e.OrganizationName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.TaxNumber).HasMaxLength(20);
            entity.Property(e => e.TaxOffice).HasMaxLength(100);
        });

        // Project Relationships
        modelBuilder.Entity<Company>()
            .HasOne(c => c.Project)
            .WithMany(p => p.Companies)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<UserProject>()
            .HasOne(up => up.Project)
            .WithMany(p => p.UserProjects)
            .HasForeignKey(up => up.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserProject>()
            .HasOne(up => up.User)
            .WithMany()
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Account Relationships
        modelBuilder.Entity<Account>()
            .HasOne(a => a.User)
            .WithOne(u => u.Account)
            .HasForeignKey<User>(u => u.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Account>()
            .HasIndex(a => a.Email)
            .IsUnique();

        modelBuilder.Entity<Account>()
            .HasIndex(a => a.Username)
            .IsUnique();

        modelBuilder.Entity<RequestHistory>(entity =>
        {
            entity.HasOne(d => d.Request)
                .WithMany(r => r.Histories)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.ActorAccount)
                .WithMany()
                .HasForeignKey(d => d.ActorAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Modular Data Seeding — only the required SystemDefaults (roles, permissions, admin, etc.).
        // Sample/demo data (SampleData) no longer lives here, see SampleDataSeeder.
        modelBuilder.SeedSystemDefaults();

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<HelpCenter.Domain.Common.IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static LambdaExpression BuildSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var propertyMethod = typeof(EF).GetMethod("Property")!.MakeGenericMethod(typeof(bool));
        var isDeletedProperty = Expression.Call(propertyMethod, parameter, Expression.Constant("IsDeleted"));
        var filterExpr = Expression.Equal(isDeletedProperty, Expression.Constant(false));
        return Expression.Lambda(filterExpr, parameter);
    }
}