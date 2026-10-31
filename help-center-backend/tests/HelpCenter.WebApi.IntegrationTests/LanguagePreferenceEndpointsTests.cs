using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HelpCenter.Persistence.Context;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class LanguagePreferenceEndpointsTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task Admin_can_read_and_persist_canonical_language_preference()
    {
        await IntegrationAuthTestData.EnsureSeededAsync(factory);
        using var client = await LoginAsync(IntegrationAuthTestData.AdminEmail, customer: false);

        using var initial = await client.GetAsync("/api/auth/language-preference");
        initial.EnsureSuccessStatusCode();
        Assert.Null(await ReadLocaleAsync(initial));

        using var update = await client.PutAsJsonAsync("/api/auth/language-preference", new { locale = "en" });
        update.EnsureSuccessStatusCode();
        Assert.Equal("en-US", await ReadLocaleAsync(update));

        using var persisted = await client.GetAsync("/api/auth/language-preference");
        Assert.Equal("en-US", await ReadLocaleAsync(persisted));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EfContext>();
        Assert.Equal("en-US", await db.Accounts.Where(account => account.Email == IntegrationAuthTestData.AdminEmail)
            .Select(account => account.PreferredLocale).SingleAsync());
        Assert.Equal("tr-TR", await db.Companies.Where(company => company.Name == "Integration test company")
            .Select(company => company.DefaultLocale).SingleAsync());
    }

    [Fact]
    public async Task Customer_can_set_preference_and_unsupported_locale_is_rejected()
    {
        await IntegrationAuthTestData.EnsureSeededAsync(factory);
        using var client = await LoginAsync(IntegrationAuthTestData.CustomerEmail, customer: true);

        using var invalid = await client.PutAsJsonAsync("/api/auth/language-preference", new { locale = "fr-FR" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using (var body = JsonDocument.Parse(await invalid.Content.ReadAsStringAsync()))
        {
            Assert.Equal("UnsupportedLocale", body.RootElement.GetProperty("code").GetString());
        }

        using var update = await client.PutAsJsonAsync("/api/auth/language-preference", new { locale = "tr" });
        update.EnsureSuccessStatusCode();
        Assert.Equal("tr-TR", await ReadLocaleAsync(update));
    }

    [Fact]
    public async Task Customer_language_preference_falls_back_to_company_default_locale()
    {
        await IntegrationAuthTestData.EnsureSeededAsync(factory);

        string previousLocale;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EfContext>();
            var customer = await db.Customers.Where(item => item.AccountId == db.Accounts
                    .Where(account => account.Email == IntegrationAuthTestData.CustomerEmail)
                    .Select(account => account.Id).Single())
                .Select(item => new { item.CompanyId, item.AccountId }).SingleAsync();
            var company = await db.Companies.SingleAsync(item => item.Id == customer.CompanyId);
            var account = await db.Accounts.SingleAsync(item => item.Id == customer.AccountId);
            previousLocale = company.DefaultLocale;
            company.DefaultLocale = "en-US";
            account.PreferredLocale = null;
            await db.SaveChangesAsync();
        }

        try
        {
            using var client = await LoginAsync(IntegrationAuthTestData.CustomerEmail, customer: true);
            using var preference = await client.GetAsync("/api/auth/language-preference");
            preference.EnsureSuccessStatusCode();
            Assert.Equal("en-US", await ReadLocaleAsync(preference));
        }
        finally
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EfContext>();
            var company = await db.Companies.SingleAsync(item => item.Name == "Integration test company");
            var account = await db.Accounts.SingleAsync(item => item.Email == IntegrationAuthTestData.CustomerEmail);
            company.DefaultLocale = previousLocale;
            account.PreferredLocale = null;
            await db.SaveChangesAsync();
        }
    }

    private async Task<HttpClient> LoginAsync(string email, bool customer)
    {
        var client = factory.CreateClient();
        var path = customer ? "/api/auth/customer-login" : "/api/auth/admin-login";
        using var response = await client.PostAsJsonAsync(path, new
        {
            email,
            password = IntegrationAuthTestData.Password
        });
        response.EnsureSuccessStatusCode();
        var token = AuthSessionEndpointsTests.ReadAccessToken(await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<string?> ReadLocaleAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("locale").ValueKind == JsonValueKind.Null
            ? null
            : body.RootElement.GetProperty("locale").GetString();
    }
}
