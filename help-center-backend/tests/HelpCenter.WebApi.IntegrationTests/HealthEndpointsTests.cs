using System.Net;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class HealthEndpointsTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task LiveAndReadyEndpoints_ReturnOkAgainstInitializedPostgreSql()
    {
        using var client = factory.CreateClient();

        var liveResponse = await client.GetAsync("/health/live");
        var readyResponse = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, liveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, readyResponse.StatusCode);
    }
}
