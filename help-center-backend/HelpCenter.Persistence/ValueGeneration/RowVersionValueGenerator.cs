using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace HelpCenter.Persistence.ValueGeneration;

public sealed class RowVersionValueGenerator : ValueGenerator<byte[]>
{
    public override bool GeneratesTemporaryValues => false;

    public override byte[] Next(EntityEntry entry) => Guid.NewGuid().ToByteArray();
}
