using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpCenter.WebApi.IntegrationTests.Infrastructure;

internal static class IntegrationAuthTestData
{
    internal const string Password = "IntegrationPassword!2026";
    internal const string AdminEmail = "integration-admin@example.test";
    internal const string CustomerEmail = "integration-customer@example.test";

    private static readonly SemaphoreSlim Gate = new(1, 1);

    internal static async Task EnsureSeededAsync(SqlServerWebApplicationFactory factory)
    {
        await Gate.WaitAsync();
        try
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EfContext>();
            if (await db.Accounts.AnyAsync(account => account.Email == AdminEmail)) return;

            var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
            var adminRole = await db.Roles.SingleAsync(role => role.Name == "Admin");
            var company = new Company { Name = "Integration test company" };
            var admin = new User
            {
                Account = CreateAccount(AdminEmail, "integration.admin", passwordService.HashPassword(Password)),
                IsActive = true
            };
            var customer = new Customer
            {
                Account = CreateAccount(CustomerEmail, "integration.customer", passwordService.HashPassword(Password)),
                Company = company,
                IsActive = true
            };

            db.AddRange(company, admin, customer);
            db.UserRoles.Add(new UserRole { User = admin, RoleId = adminRole.Id, IsActive = true });
            await db.SaveChangesAsync();
        }
        finally
        {
            Gate.Release();
        }
    }

    private static Account CreateAccount(string email, string username, string passwordHash) => new()
    {
        Email = email,
        Username = username,
        FirstName = "Integration",
        LastName = "Test",
        Password = passwordHash,
        IsActive = true
    };
}
