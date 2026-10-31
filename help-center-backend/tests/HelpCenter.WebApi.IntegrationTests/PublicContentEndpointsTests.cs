using System.Net;
using System.Text.Json;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class PublicContentEndpointsTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task PublicFaqs_ApplyModuleAndSearchFilters_BeforePaginating()
    {
        var seed = await SeedPublicContentAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/public/faq?moduleId={seed.FaqModuleId}&search=smoke-faq-filter&pageNumber=2&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;
        Assert.Equal(2, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, root.GetProperty("totalPages").GetInt32());
        Assert.Equal(2, root.GetProperty("currentPage").GetInt32());
        Assert.Equal(1, root.GetProperty("pageSize").GetInt32());

        var items = root.GetProperty("items");
        Assert.Equal(1, items.GetArrayLength());
        Assert.Equal("Smoke FAQ Filter Two", items[0].GetProperty("title").GetString());
        Assert.Equal(seed.FaqModuleId, Guid.Parse(items[0].GetProperty("moduleId").GetString()!));
    }

    [Fact]
    public async Task PublicGuides_ApplyProjectModuleAndSearchFilters_BeforePaginating()
    {
        var seed = await SeedPublicContentAsync();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/public/guides?projectId={seed.ProjectId}&module={Uri.EscapeDataString(seed.GuideModuleName)}&search=smoke-guide-filter&pageNumber=2&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());
        var root = document.RootElement;
        Assert.Equal(2, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(2, root.GetProperty("totalPages").GetInt32());
        Assert.Equal(2, root.GetProperty("currentPage").GetInt32());
        Assert.Equal(1, root.GetProperty("pageSize").GetInt32());

        var items = root.GetProperty("items");
        Assert.Equal(1, items.GetArrayLength());
        Assert.Equal("Smoke Guide Filter Two", items[0].GetProperty("title").GetString());
        Assert.Equal(seed.GuideModuleName, items[0].GetProperty("module").GetString());
    }

    [Fact]
    public async Task PublicGuides_DoNotReturnSourceLanguageDocumentsForAnotherRequestedLocale()
    {
        var unique = Guid.NewGuid().ToString("N");
        var title = $"QA source-only guide {unique}";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EfContext>();
            db.Guides.Add(new Guide
            {
                Title = title,
                Description = "Turkish source content",
                SourceLocale = "tr-TR",
                Module = "QA",
                IsPublic = true,
                IsActive = true,
                Documents = new List<Document>
                {
                    new() { Path = $"/qa/source-only-{unique}.pdf", FileName = "source-only.pdf" }
                }
            });
            await db.SaveChangesAsync();
        }

        try
        {
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US");
            using var englishResponse = await client.GetAsync(
                $"/api/public/guides?search={Uri.EscapeDataString(title)}&pageNumber=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, englishResponse.StatusCode);
            using var englishBody = JsonDocument.Parse(await englishResponse.Content.ReadAsStringAsync());
            var englishGuide = englishBody.RootElement.GetProperty("items").EnumerateArray().Single();
            Assert.Equal("en-US", englishGuide.GetProperty("requestedLocale").GetString());
            Assert.Equal("tr-TR", englishGuide.GetProperty("contentLocale").GetString());
            Assert.Empty(englishGuide.GetProperty("documents").EnumerateArray());

            client.DefaultRequestHeaders.AcceptLanguage.Clear();
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("tr-TR");
            using var turkishResponse = await client.GetAsync(
                $"/api/public/guides?search={Uri.EscapeDataString(title)}&pageNumber=1&pageSize=10");
            Assert.Equal(HttpStatusCode.OK, turkishResponse.StatusCode);
            using var turkishBody = JsonDocument.Parse(await turkishResponse.Content.ReadAsStringAsync());
            var turkishGuide = turkishBody.RootElement.GetProperty("items").EnumerateArray().Single();
            Assert.Equal("tr-TR", turkishGuide.GetProperty("requestedLocale").GetString());
            Assert.Single(turkishGuide.GetProperty("documents").EnumerateArray());
            Assert.Equal($"/qa/source-only-{unique}.pdf",
                turkishGuide.GetProperty("documents")[0].GetProperty("path").GetString());
        }
        finally
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EfContext>();
            var guide = db.Guides.Single(item => item.Title == title);
            db.Remove(guide);
            await db.SaveChangesAsync();
        }
    }

    private async Task<PublicContentSeed> SeedPublicContentAsync()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EfContext>();

        var project = Project.Create("Integration public content project", "Test-owned public content project");
        var otherProject = Project.Create("Integration other public content project", "Test-owned non-matching project");
        var faqModule = Module.Create("Integration FAQ module", "Test-owned FAQ module");
        var guideModule = Module.Create("Integration guide module", "Test-owned guide module");
        dbContext.AddRange(project, otherProject, faqModule, guideModule);
        await dbContext.SaveChangesAsync();

        dbContext.ProjectModules.Add(new ProjectModule
        {
            ProjectId = project.Id,
            ModuleId = guideModule.Id
        });

        dbContext.FAQs.AddRange(
            CreateFaq("Smoke FAQ Filter One", faqModule.Id, project.Id),
            CreateFaq("Smoke FAQ Filter Two", faqModule.Id, project.Id),
            CreateFaq("Smoke FAQ Filter Wrong Module", guideModule.Id, project.Id),
            CreateFaq("Smoke FAQ Filter Hidden", faqModule.Id, project.Id, isPublic: false),
            CreateFaq("Smoke FAQ Filter Inactive", faqModule.Id, project.Id, isActive: false));

        dbContext.Guides.AddRange(
            CreateGuide("Smoke Guide Filter One", guideModule.Name, project.Id),
            CreateGuide("Smoke Guide Filter Two", guideModule.Name, project.Id),
            CreateGuide("Smoke Guide Filter Wrong Project", guideModule.Name, otherProject.Id),
            CreateGuide("Smoke Guide Filter Hidden", guideModule.Name, project.Id, isPublic: false),
            CreateGuide("Smoke Guide Filter Inactive", guideModule.Name, project.Id, isActive: false));

        await dbContext.SaveChangesAsync();

        return new PublicContentSeed(project.Id, faqModule.Id, guideModule.Name);
    }

    private static FAQ CreateFaq(string title, Guid moduleId, Guid projectId, bool isPublic = true, bool isActive = true) => new()
    {
        Title = title,
        Description = "smoke-faq-filter",
        ModuleId = moduleId,
        ProjectId = projectId,
        IsPublic = isPublic,
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow
    };

    private static Guide CreateGuide(string title, string module, Guid? projectId, bool isPublic = true, bool isActive = true) => new()
    {
        Title = title,
        Description = "smoke-guide-filter",
        Module = module,
        ProjectId = projectId,
        IsPublic = isPublic,
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow
    };

    private sealed record PublicContentSeed(Guid ProjectId, Guid FaqModuleId, string GuideModuleName);
}
