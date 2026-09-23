using System.Net;
using System.Text.Json;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class PublicContentEndpointsTests(SqlServerWebApplicationFactory factory)
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
        Assert.Equal(seed.FaqModuleId, items[0].GetProperty("moduleId").GetInt32());
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

    private static FAQ CreateFaq(string title, int moduleId, int projectId, bool isPublic = true, bool isActive = true) => new()
    {
        Title = title,
        Description = "smoke-faq-filter",
        ModuleId = moduleId,
        ProjectId = projectId,
        IsPublic = isPublic,
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow
    };

    private static Guide CreateGuide(string title, string module, int? projectId, bool isPublic = true, bool isActive = true) => new()
    {
        Title = title,
        Description = "smoke-guide-filter",
        Module = module,
        ProjectId = projectId,
        IsPublic = isPublic,
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow
    };

    private sealed record PublicContentSeed(int ProjectId, int FaqModuleId, string GuideModuleName);
}
