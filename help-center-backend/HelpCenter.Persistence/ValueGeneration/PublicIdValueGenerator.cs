using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace HelpCenter.Persistence.ValueGeneration;

/// <summary>
/// Client-side generator for BaseEntity.PublicId. Assigns a real, random Guid to every entity
/// added (via Add) through the EF SaveChanges pipeline — on providers such as InMemory/SQLite,
/// HasDefaultValueSql cannot fill the generated row during the normal SaveChanges flow (same
/// rationale as RowVersionValueGenerator), so a separate client-side generator is needed here too.
/// On SQL Server, the NEWID() default is already in effect in the real/prod flow; this generator
/// only kicks in as part of the normal EF Core value-generation flow for entities added
/// via SaveChanges.
/// </summary>
public sealed class PublicIdValueGenerator : ValueGenerator<Guid>
{
    public override bool GeneratesTemporaryValues => false;

    public override Guid Next(EntityEntry entry) => Guid.NewGuid();
}
