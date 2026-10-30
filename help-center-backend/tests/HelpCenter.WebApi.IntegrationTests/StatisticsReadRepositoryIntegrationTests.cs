using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Context.Seed.SampleData;
using HelpCenter.Persistence.Context.Seed.SystemDefaults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

public sealed class StatisticsReadRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("HelpCenterStatisticsTests")
        .WithUsername("postgres")
        .WithPassword("IntegrationTests!2026")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();

    [Fact]
    public async Task DashboardReadModel_ReturnsPostgreSqlAggregatesWithTheExistingContracts()
    {
        var services = new ServiceCollection()
            .AddDbContext<EfContext>(options => options.UseNpgsql(
                _postgres.GetConnectionString() + ";Timezone=America/Los_Angeles"))
            .AddScoped<IStatisticsReadRepository, HelpCenter.Persistence.Repositories.StatisticsReadRepository>()
            .BuildServiceProvider();

        try
        {
            await using (var setupScope = services.CreateAsyncScope())
            {
                var db = setupScope.ServiceProvider.GetRequiredService<EfContext>();
                await db.Database.MigrateAsync();
                await SystemDefaultsSeeder.SeedAsync(db);
                await SampleDataSeeder.SeedAsync(db);

                var todayUtc = DateTime.UtcNow.Date;
                var now = todayUtc.AddHours(3);
                var request = new CustomerRequest
                {
                    TicketId = "TKT-P4-STATISTICS",
                    CustomerId = TestIds.FromInt(1),
                    ModuleId = TestIds.FromInt(1),
                    Title = "Statistics integration test",
                    Description = "P4 PostgreSQL dashboard coverage",
                    StatusId = RequestStatusConstants.Completed,
                    AssignedUserId = TestIds.FromInt(2),
                    CreatedAt = todayUtc.AddHours(1).AddSeconds(30),
                    UpdatedAt = todayUtc.AddHours(2).AddMinutes(1),
                    IsActive = true
                };
                var boundaryRequest = new CustomerRequest
                {
                    TicketId = "TKT-P4-MINUTE-BOUNDARY",
                    CustomerId = TestIds.FromInt(1),
                    ModuleId = TestIds.FromInt(1),
                    Title = "Minute boundary integration test",
                    Description = "Preserve SQL Server minute-boundary averaging",
                    StatusId = RequestStatusConstants.Completed,
                    AssignedUserId = TestIds.FromInt(2),
                    CreatedAt = todayUtc.AddMinutes(59),
                    UpdatedAt = todayUtc.AddHours(1),
                    IsActive = true
                };

                db.CustomerRequests.AddRange(request, boundaryRequest);
                db.RequestHistories.Add(new RequestHistory
                {
                    RequestId = request.Id,
                    ActorAccountId = TestIds.FromInt(2),
                    Action = RequestActionType.StatusChanged,
                    OldValue = RequestStatusConstants.Completed.ToString("D"),
                    CreatedAt = now,
                    IsActive = true
                });
                await db.SaveChangesAsync();
            }

            await using var readScope = services.CreateAsyncScope();
            var statistics = readScope.ServiceProvider.GetRequiredService<IStatisticsReadRepository>();

            var dashboard = await statistics.GetDashboardStatsAsync();
            Assert.Equal(5, dashboard["TotalRequests"]);
            Assert.Equal(2, dashboard["ActiveRequests"]);
            Assert.Equal(3, dashboard["CompletedRequests"]);
            Assert.Equal(5, dashboard["TotalMessages"]);
            Assert.Equal(4, dashboard["TotalCustomers"]);
            Assert.Equal(3, dashboard["TotalCompanies"]);

            var statuses = await statistics.GetStatusDistributionAsync();
            Assert.Equal(3, Assert.Single(statuses, row => row.Id == RequestStatusConstants.Completed).Count);

            var modules = await statistics.GetModuleDistributionAsync();
            Assert.Equal(3, Assert.Single(modules, row => row.Id == TestIds.FromInt(1)).Count);

            var companies = await statistics.GetCompanyTopNAsync();
            Assert.Equal(3, companies.Count);
            Assert.Equal(5, companies.Sum(row => row.Count));

            var dailyTrend = await statistics.GetDailyTrendAsync();
            Assert.Equal(30, dailyTrend.Count);
            var today = Assert.Single(dailyTrend, row => row.Date.Date == DateTime.UtcNow.Date);
            Assert.Equal(2, today.Opened);
            Assert.Equal(2, today.Closed);

            Assert.Equal(31, await statistics.GetAvgResolutionMinutesAsync());

            var agent = Assert.Single(await statistics.GetAgentPerformanceAsync());
            Assert.Equal(TestIds.FromInt(2), agent.UserId);
            Assert.Equal(5, agent.Assigned);
            Assert.Equal(3, agent.Completed);
            Assert.Equal(31, agent.AvgResolutionMinutes);

            var reopenStats = await statistics.GetReopenStatsAsync();
            Assert.Equal(3, reopenStats.TotalCompleted);
            Assert.Equal(1, reopenStats.Reopened);
            Assert.Equal(33, reopenStats.RatePercent);

            var weeklyOpened = await statistics.GetWeeklyOpenedAsync(14);
            Assert.Equal(14, weeklyOpened.Count);
            Assert.Equal(2, Assert.Single(weeklyOpened, row => row.Date.Date == DateTime.UtcNow.Date).Count);

            var recent = await statistics.GetRecentRequestsAsync(7);
            Assert.Contains(recent, row => row.TicketId == "TKT-P4-STATISTICS");
        }
        finally
        {
            await services.DisposeAsync();
        }
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();
}
