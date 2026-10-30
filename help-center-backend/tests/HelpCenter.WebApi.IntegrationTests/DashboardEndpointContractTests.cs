using System.Net;
using System.Text.Json;
using HelpCenter.Domain.Constants;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class DashboardEndpointContractTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task StatisticsAndReportsEndpoints_KeepTheirExistingResponseFields()
    {
        var (client, _, _) = await StaffTestSession.CreateAsync(factory, AppResources.Dashboard);
        using (client)
        {
            var statisticsResponse = await client.GetAsync("/api/admin/statistics");
            Assert.Equal(HttpStatusCode.OK, statisticsResponse.StatusCode);
            using var statisticsJson = JsonDocument.Parse(await statisticsResponse.Content.ReadAsStringAsync());
            var statistics = statisticsJson.RootElement;

            Assert.Equal(JsonValueKind.Number, statistics.GetProperty("totalRequests").ValueKind);
            Assert.Equal(JsonValueKind.Number, statistics.GetProperty("activeRequests").ValueKind);
            Assert.Equal(JsonValueKind.Number, statistics.GetProperty("completedRequests").ValueKind);
            Assert.Equal(JsonValueKind.Number, statistics.GetProperty("totalMessages").ValueKind);
            Assert.Equal(JsonValueKind.Number, statistics.GetProperty("totalCustomers").ValueKind);
            Assert.Equal(JsonValueKind.Number, statistics.GetProperty("totalCompanies").ValueKind);

            var reportsResponse = await client.GetAsync("/api/admin/reports");
            Assert.Equal(HttpStatusCode.OK, reportsResponse.StatusCode);
            using var reportsJson = JsonDocument.Parse(await reportsResponse.Content.ReadAsStringAsync());
            var reports = reportsJson.RootElement;

            Assert.Equal(JsonValueKind.Object, reports.GetProperty("dashboard").ValueKind);
            Assert.Equal(JsonValueKind.Array, reports.GetProperty("statusDistribution").ValueKind);
            Assert.Equal(JsonValueKind.Array, reports.GetProperty("moduleDistribution").ValueKind);
            Assert.Equal(JsonValueKind.Array, reports.GetProperty("companyTopN").ValueKind);
            Assert.Equal(JsonValueKind.Array, reports.GetProperty("dailyTrend").ValueKind);
            Assert.Equal(JsonValueKind.Number, reports.GetProperty("avgResolutionMinutes").ValueKind);
            Assert.Equal(JsonValueKind.Array, reports.GetProperty("agentPerformance").ValueKind);
            Assert.Equal(JsonValueKind.Object, reports.GetProperty("reopenStats").ValueKind);
        }
    }
}
