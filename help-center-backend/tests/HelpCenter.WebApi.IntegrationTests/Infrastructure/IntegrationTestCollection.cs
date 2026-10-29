using Xunit;

namespace HelpCenter.WebApi.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<PostgreSqlWebApplicationFactory>
{
    public const string Name = "PostgreSQL integration tests";
}
