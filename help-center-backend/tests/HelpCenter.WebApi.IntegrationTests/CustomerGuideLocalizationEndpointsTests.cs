using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class CustomerGuideLocalizationEndpointsTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task Customer_guides_use_requested_and_company_fallback_locales_without_cross_language_documents()
    {
        await IntegrationAuthTestData.EnsureSeededAsync(factory);
        var unique = Guid.NewGuid().ToString("N");
        var localizedTitle = $"QA localized guide {unique}";
        var companyFallbackTitle = $"QA company fallback guide {unique}";
        string previousLocale;

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EfContext>();
            var customer = await db.Customers.Where(item => item.AccountId == db.Accounts
                    .Where(account => account.Email == IntegrationAuthTestData.CustomerEmail)
                    .Select(account => account.Id).Single())
                .Select(item => item.CompanyId).SingleAsync();
            var company = await db.Companies.SingleAsync(item => item.Id == customer);
            previousLocale = company.DefaultLocale;
            company.DefaultLocale = "en-US";

            var localizedGuide = new Guide
            {
                Title = $"Kaynak başlık {unique}",
                Description = "Kaynak açıklama",
                SourceLocale = "tr-TR",
                Module = "QA",
                Documents = new List<Document> { new() { Path = "/qa/qa-tr.pdf", FileName = "qa-tr.pdf" } }
            };
            localizedGuide.Translations.Add(new GuideTranslation
            {
                Locale = "tr-TR",
                Title = localizedTitle,
                Description = "Türkçe kılavuz",
                Documents = new List<GuideTranslationDocument>()
            });
            localizedGuide.Translations.Add(new GuideTranslation
            {
                Locale = "en-US",
                Title = $"English guide {unique}",
                Description = "English guide content",
                Documents = new List<GuideTranslationDocument>
                {
                    new() { Path = "/qa/qa-en.pdf", FileName = "qa-en.pdf" }
                }
            });

            var companyFallbackGuide = new Guide
            {
                Title = $"Kaynak fallback {unique}",
                Description = "Türkçe kaynak içerik",
                SourceLocale = "tr-TR",
                Module = "QA"
            };
            companyFallbackGuide.Translations.Add(new GuideTranslation
            {
                Locale = "en-US",
                Title = companyFallbackTitle,
                Description = "Company default translation",
                Documents = new List<GuideTranslationDocument>()
            });

            db.Guides.AddRange(localizedGuide, companyFallbackGuide);
            await db.SaveChangesAsync();
        }

        try
        {
            using var client = await LoginCustomerAsync();
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
            using var englishResponse = await client.GetAsync("/api/customer/guides/get-all");
            Assert.Equal(HttpStatusCode.OK, englishResponse.StatusCode);
            using var englishBody = JsonDocument.Parse(await englishResponse.Content.ReadAsStringAsync());
            var englishGuide = FindByTitle(englishBody.RootElement, $"English guide {unique}");
            Assert.Equal("en-US", englishGuide.GetProperty("requestedLocale").GetString());
            Assert.Equal("en-US", englishGuide.GetProperty("contentLocale").GetString());
            Assert.Equal("/qa/qa-en.pdf", englishGuide.GetProperty("documents")[0].GetProperty("path").GetString());
            Assert.Empty(FindByTitle(englishBody.RootElement, companyFallbackTitle).GetProperty("documents").EnumerateArray());

            client.DefaultRequestHeaders.AcceptLanguage.Clear();
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("tr-TR"));
            using var turkishResponse = await client.GetAsync("/api/customer/guides/get-all");
            Assert.Equal(HttpStatusCode.OK, turkishResponse.StatusCode);
            using var turkishBody = JsonDocument.Parse(await turkishResponse.Content.ReadAsStringAsync());
            var turkishGuide = FindByTitle(turkishBody.RootElement, localizedTitle);
            Assert.Equal("tr-TR", turkishGuide.GetProperty("requestedLocale").GetString());
            Assert.Equal("tr-TR", turkishGuide.GetProperty("contentLocale").GetString());
            Assert.Equal("/qa/qa-tr.pdf", turkishGuide.GetProperty("documents")[0].GetProperty("path").GetString());

            var companyFallback = FindByTitle(turkishBody.RootElement, companyFallbackTitle);
            Assert.Equal("tr-TR", companyFallback.GetProperty("requestedLocale").GetString());
            Assert.Equal("en-US", companyFallback.GetProperty("contentLocale").GetString());
            Assert.Equal("Company default translation", companyFallback.GetProperty("description").GetString());
            Assert.Empty(companyFallback.GetProperty("documents").EnumerateArray());
        }
        finally
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EfContext>();
            var company = await db.Companies.SingleAsync(item => item.Name == "Integration test company");
            company.DefaultLocale = previousLocale;
            var guides = await db.Guides.Where(guide => guide.Title.Contains(unique)).ToListAsync();
            db.RemoveRange(guides);
            await db.SaveChangesAsync();
        }
    }

    private async Task<HttpClient> LoginCustomerAsync()
    {
        var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/auth/customer-login", new
        {
            email = IntegrationAuthTestData.CustomerEmail,
            password = IntegrationAuthTestData.Password
        });
        response.EnsureSuccessStatusCode();
        var token = AuthSessionEndpointsTests.ReadAccessToken(await response.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static JsonElement FindByTitle(JsonElement body, string title) =>
        body.EnumerateArray().Single(guide => guide.GetProperty("title").GetString() == title);
}
