namespace HelpCenter.WebApi.IntegrationTests;

internal static class TestIds
{
    public static Guid FromInt(int value) => value > 0
        ? Guid.Parse($"00000000-0000-0000-0000-{value:D12}")
        : Guid.Empty;
}
