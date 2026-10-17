using System.Net.Http.Headers;
using System.Net.Http.Json;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;

namespace HelpCenter.WebApi.IntegrationTests.Infrastructure;

internal static class StaffTestSession
{
    internal static async Task<(HttpClient Client, Guid UserId, Guid RoleId)> CreateAsync(SqlServerWebApplicationFactory factory, string resource)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EfContext>();
        var suffix = Guid.NewGuid().ToString("N");
        var role = new Role { Name = "test-" + suffix };
        var user = new User { Account = new Account { Email = suffix + "@example.test", Username = suffix,
            FirstName = "Test", LastName = "Staff", Password = scope.ServiceProvider.GetRequiredService<IPasswordService>().HashPassword(IntegrationAuthTestData.Password) } };
        db.UserRoles.Add(new UserRole { User = user, Role = role });
        if (!string.IsNullOrEmpty(resource)) db.RolePermissions.Add(new RolePermission { Role = role, ResourceKey = resource,
            Actions = new List<RolePermissionAction> { new() { Action = "Read" } } });
        await db.SaveChangesAsync();
        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/admin-login", new { email = user.Account.Email, password = IntegrationAuthTestData.Password });
        login.EnsureSuccessStatusCode();
        var token = AuthSessionEndpointsTests.ReadAccessToken(await login.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, user.Id, role.Id);
    }
}
