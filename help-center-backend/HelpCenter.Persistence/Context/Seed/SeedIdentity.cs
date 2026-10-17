namespace HelpCenter.Persistence.Context.Seed;

/// <summary>Stable identifiers for model and sample seed rows while IDs are GUID based.</summary>
internal static class SeedIdentity
{
    public static Guid FromLegacyInt(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        return Guid.Parse($"00000000-0000-0000-0000-{value:D12}");
    }
}
