using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuditLogAppendOnlyIntegrationTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task PostgreSql_RejectsAuditLogUpdatesAndDeletes()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<EfContext>();
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            EventType = "AppendOnlyIntegrationTest",
            OccurredAt = DateTime.UtcNow
        };

        db.AuditLogs.Add(auditLog);
        await db.SaveChangesAsync();

        var updateException = await Assert.ThrowsAsync<PostgresException>(() =>
            db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE \"AuditLogs\" SET \"EventType\" = {"Modified"} WHERE \"Id\" = {auditLog.Id}"));
        Assert.Equal("55000", updateException.SqlState);

        var deleteException = await Assert.ThrowsAsync<PostgresException>(() =>
            db.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM \"AuditLogs\" WHERE \"Id\" = {auditLog.Id}"));
        Assert.Equal("55000", deleteException.SqlState);

        var stored = await db.AuditLogs.AsNoTracking().SingleAsync(x => x.Id == auditLog.Id);
        Assert.Equal("AppendOnlyIntegrationTest", stored.EventType);
    }
}
