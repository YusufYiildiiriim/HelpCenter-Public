using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace HelpCenter.Persistence.ValueGeneration;

/// <summary>
/// Fallback for explicitly empty BaseEntity.PublicId values during EF tracking. Normal runtime
/// entities already have a stable identity from construction; EF preserves non-empty values.
/// HasData rows bypass this generator and use the provider's database default.
/// </summary>
public sealed class PublicIdValueGenerator : ValueGenerator<Guid>
{
    public override bool GeneratesTemporaryValues => false;

    public override Guid Next(EntityEntry entry) => Guid.NewGuid();
}
