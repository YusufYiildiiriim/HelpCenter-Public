using Xunit;

namespace HelpCenter.WebApi.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<SqlServerWebApplicationFactory>
{
    public const string Name = "SQL Server integration tests";
}
