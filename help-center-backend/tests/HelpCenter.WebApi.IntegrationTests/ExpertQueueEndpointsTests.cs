using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HelpCenter.Domain.Constants;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using HelpCenter.Persistence.Context;
using HelpCenter.WebApi.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.WebApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class ExpertQueueEndpointsTests(PostgreSqlWebApplicationFactory factory)
{
    [Fact]
    public async Task Consultation_persists_and_is_visible_only_to_the_selected_expert_session()
    {
        await IntegrationAuthTestData.EnsureSeededAsync(factory);
        var expert = await StaffTestSession.CreateAsync(factory, AppResources.AssignedRequests);
        var other = await StaffTestSession.CreateAsync(factory, AppResources.AssignedRequests);
        using var expertClient = expert.Client;
        using var otherClient = other.Client;
        Guid publicId;
        Guid adminId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EfContext>();
            var customer = await db.Customers.FirstAsync(x => x.Account.Email == IntegrationAuthTestData.CustomerEmail);
            adminId = (await db.Users.SingleAsync(x => x.Account.Email == IntegrationAuthTestData.AdminEmail)).Id;
            var request = CustomerRequest.Create(customer.Id, null, null, "Expert queue integration", RequestPriority.Medium);
            db.CustomerRequests.Add(request);
            await db.SaveChangesAsync();
            publicId = request.PublicId;
        }
        using var admin = factory.CreateClient();
        using var login = await new AuthSessionEndpointsTests(factory).LoginAdminAsync();
        admin.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthSessionEndpointsTests.ReadAccessToken(await login.Content.ReadAsStringAsync()));
        var consult = await admin.PostAsJsonAsync("/api/admin/requests/consult-expert", new { requestPublicId = publicId, expertId = expert.UserId, agentUserId = adminId, note = "Inspect" });
        consult.EnsureSuccessStatusCode();
        var queue = await expertClient.GetFromJsonAsync<JsonElement>("/api/admin/requests/assigned");
        Assert.Contains(queue.GetProperty("items").EnumerateArray(), x => x.GetProperty("publicId").GetGuid() == publicId);
        (await expertClient.GetAsync($"/api/admin/requests/assigned/{publicId}")).EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.NotFound, (await otherClient.GetAsync($"/api/admin/requests/assigned/{publicId}")).StatusCode);
        var warm = await expertClient.GetFromJsonAsync<JsonElement>($"/api/admin/requests/assigned/{publicId}/messages?pageSize=10");
        Assert.Single(warm.GetProperty("messages").EnumerateArray());
        (await admin.PostAsJsonAsync("/api/admin/requests/consult-expert", new { requestPublicId = publicId, expertId = expert.UserId, agentUserId = adminId, note = "Inspect again" })).EnsureSuccessStatusCode();
        var refreshed = await expertClient.GetFromJsonAsync<JsonElement>($"/api/admin/requests/assigned/{publicId}/messages?pageSize=10");
        Assert.Equal(2, refreshed.GetProperty("messages").GetArrayLength());
        Assert.Contains(refreshed.GetProperty("messages").EnumerateArray(), x => x.GetProperty("messageText").GetString()!.Contains("Inspect again"));
        using var verifyScope = factory.Services.CreateScope();
        var persisted = await verifyScope.ServiceProvider.GetRequiredService<EfContext>().CustomerRequests.SingleAsync(x => x.PublicId == publicId);
        Assert.Equal(expert.UserId, persisted.CurrentExpertId);
        Assert.Equal(RequestStatusConstants.UnderTechnicalReview, persisted.StatusId);
    }
}
