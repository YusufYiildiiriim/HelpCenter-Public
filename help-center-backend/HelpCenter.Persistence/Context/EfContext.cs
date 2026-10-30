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
        // Provider-specific column literals are intentionally absent from the active model.
        var isPostgreSql = Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

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

                // Application-managed opaque concurrency token, rotated before updates.
                modelBuilder.Entity(entityType.ClrType)
                    .Property("RowVersion")
                    .IsConcurrencyToken()
                    .ValueGeneratedNever();

                // PublicId Configuration — a real/randomly generated, stored external identity
                // for each entity, replacing the XOR/HMAC scheme that "obfuscated" the Guid Id
                // reversibly with a fixed salt embedded in HashService's source code.
                // Runtime entities create their identity in BaseEntity. Keep the value generator
                // as a fallback for explicitly empty IDs, and the database default for HasData
                // rows or SQL inserts that omit PublicId. Existing non-empty values are preserved.
                modelBuilder.Entity(entityType.ClrType).Property<Guid>(nameof(BaseEntity.Id)).ValueGeneratedNever();

                var publicIdProperty = modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.PublicId))
                    .ValueGeneratedOnAdd()
                    .HasValueGenerator<HelpCenter.Persistence.ValueGeneration.PublicIdValueGenerator>();

                if (isPostgreSql)
                    publicIdProperty.HasDefaultValueSql("gen_random_uuid()");

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

        modelBuilder.Entity<Guide>().HasOne(g => g.ModuleEntity).WithMany().HasForeignKey(g => g.ModuleId).OnDelete(DeleteBehavior.Restrict);

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

        // System defaults are inserted by SystemDefaultsSeedHostedService at runtime. Keeping
        // them out of HasData prevents EF migrations from overwriting environment-specific IDs
        // or customized permission/admin data.

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges() => SaveChanges(acceptAllChangesOnSuccess: true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PrepareEntitiesForSave();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(acceptAllChangesOnSuccess: true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        PrepareEntitiesForSave();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void PrepareEntitiesForSave()
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.RowVersion = Guid.NewGuid();
        }

        foreach (var entry in ChangeTracker.Entries<HelpCenter.Domain.Common.IAuditable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedAt == default)
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        // PostgreSQL timestamptz values must be UTC. DateTimeKind.Unspecified is treated
        // as UTC by this application's persistence contract; local values are converted.
        // This also covers business timestamps such as ConversationParticipant.JoinedAt,
        // which are not part of IAuditable and therefore bypass the audit timestamp handling.
        foreach (var entry in ChangeTracker.Entries()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            foreach (var property in entry.Entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                         .Where(property => property.CanRead && property.CanWrite))
            {
                var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                if (propertyType != typeof(DateTime) || property.GetValue(entry.Entity) is not DateTime value)
                    continue;

                var utcValue = value.Kind switch
                {
                    DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                    DateTimeKind.Local => value.ToUniversalTime(),
                    _ => value
                };
                property.SetValue(entry.Entity, utcValue);
            }
        }

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
