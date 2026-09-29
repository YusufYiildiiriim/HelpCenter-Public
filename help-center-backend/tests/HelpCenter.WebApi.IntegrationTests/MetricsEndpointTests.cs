using System.Net;
using System.Net.Http.Headers;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class MetricsEndpointTests(SqlServerWebApplicationFactory factory)
{
    [Fact]
    public async Task Metrics_rejects_anonymous_requests()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/metrics");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Metrics_allows_an_authenticated_scrape_and_enforces_its_named_limit()
    {
        var token = await LoginAndReadAccessTokenAsync();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        for (var requestNumber = 0; requestNumber < 30; requestNumber++)
        {
            using var response = await client.GetAsync("/metrics");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            if (requestNumber == 0)
            {
                Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
                Assert.Contains("# TYPE ", await response.Content.ReadAsStringAsync());
            }
        }

        using var limitedResponse = await client.GetAsync("/metrics");
        Assert.Equal(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
    }

    private async Task<string> LoginAndReadAccessTokenAsync()
    {
        var sessions = new AuthSessionEndpointsTests(factory);
        using var login = await sessions.LoginAdminAsync();
        var token = AuthSessionEndpointsTests.ReadAccessToken(await login.Content.ReadAsStringAsync());

        return Assert.IsType<string>(token);
    }
}
