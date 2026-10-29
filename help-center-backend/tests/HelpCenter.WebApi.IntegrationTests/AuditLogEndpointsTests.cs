using System.Net;
using System.Text.Json;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class AuditLogEndpointsTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task Audit_read_requires_explicit_permission_and_limits_fields_and_page_size()
    {
        using var anonymous = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/admin/audit-logs")).StatusCode);
        var denied = await StaffTestSession.CreateAsync(factory, "");
        using var deniedClient = denied.Client;
        Assert.Equal(HttpStatusCode.Forbidden, (await deniedClient.GetAsync("/api/admin/audit-logs")).StatusCode);
        var allowed = await StaffTestSession.CreateAsync(factory, AppResources.AuditLogs);
        using var client = allowed.Client;
        var marker = "TestAudit" + Guid.NewGuid().ToString("N");
        var firstTargetId = Guid.NewGuid();
        var secondTargetId = Guid.NewGuid();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EfContext>();
            db.AuditLogs.AddRange(new AuditLog { EventType = marker, ActorUserId = allowed.UserId, ActorEmail = "private@example.test", IpAddress = "private-ip", BeforeJson = "secret-before", MetadataJson = "secret-metadata", TargetType = "Role", TargetId = firstTargetId },
                new AuditLog { EventType = marker, ActorUserId = allowed.UserId, TargetType = "Role", TargetId = secondTargetId });
            await db.SaveChangesAsync();
        }
        var response = await client.GetAsync($"/api/admin/audit-logs?eventType={marker}&actorUserId={allowed.UserId}&targetType=Role&pageSize=1&pageNumber=1");
        response.EnsureSuccessStatusCode();
        var text = await response.Content.ReadAsStringAsync();
        using var body = JsonDocument.Parse(text);
        Assert.Equal(2, body.RootElement.GetProperty("totalCount").GetInt32());
        var row = Assert.Single(body.RootElement.GetProperty("items").EnumerateArray());
        Assert.Contains(Guid.Parse(row.GetProperty("targetId").GetString()!), new[] { firstTargetId, secondTargetId });
        Assert.DoesNotContain("private", text);
        Assert.DoesNotContain("secret", text);
        Assert.False(row.TryGetProperty("beforeJson", out _));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/admin/audit-logs?pageSize=0")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/admin/audit-logs?pageSize=101")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/admin/audit-logs?from=2026-10-17&to=2026-10-16")).StatusCode);
    }
}
