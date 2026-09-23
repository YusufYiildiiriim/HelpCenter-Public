using FluentAssertions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Constants;
using HelpCenter.Persistence.Context.Seed.SampleData;
using HelpCenter.Persistence.Services;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Authorization;

public class RbacPermissionTests : HandlerTestBase
{
    public RbacPermissionTests()
    {
        Db.Database.EnsureCreated();

        // Agent (User 2) and Expert (User 4) come from the sample/demo data. Since this data is no
        // longer embedded in migrations, EnsureCreated alone is not enough; tests are treated as
        // equivalent to the Development environment, and the same runtime seed path is explicitly
        // called here as well.
        SampleDataSeeder.SeedAsync(Db).GetAwaiter().GetResult();
    }

    private DataScopeService CreateScopeServiceForUser(int userId)
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(userId);
        return new DataScopeService(Uow, userContext);
    }

    [Fact]
    public async Task Admin_should_have_full_crud_permissions_across_all_resources()
    {
        // User 1: Admin
        var scope = CreateScopeServiceForUser(1);

        var resources = new[]
        {
            AppResources.Dashboard,
            AppResources.Requests,
            AppResources.Companies,
            AppResources.Users,
            AppResources.Roles,
            AppResources.Modules,
            AppResources.Projects,
            AppResources.OrganizationSettings
        };

        foreach (var resource in resources)
        {
            var canRead = await scope.HasPermissionAsync(resource, PermissionActions.Read);
            canRead.Should().BeTrue($"Admin should be able to Read {resource}");
        }

        var canCreateCompany = await scope.HasPermissionAsync(AppResources.Companies, PermissionActions.Create);
        var canDeleteUser = await scope.HasPermissionAsync(AppResources.Users, PermissionActions.Delete);
        canCreateCompany.Should().BeTrue();
        canDeleteUser.Should().BeTrue();
    }

    [Fact]
    public async Task Admin_should_have_unrestricted_allowed_fields_on_dashboard()
    {
        // User 1: Admin
        var scope = CreateScopeServiceForUser(1);
        var allowedFields = await scope.GetAllowedFieldsAsync(AppResources.Dashboard);

        // Null means all fields are unrestricted
        allowedFields.Should().BeNull();
    }

    [Fact]
    public async Task Agent_should_have_read_and_update_on_requests_and_companies_but_not_delete()
    {
        // User 2: Agent 1
        var scope = CreateScopeServiceForUser(2);

        var canReadRequests = await scope.HasPermissionAsync(AppResources.Requests, PermissionActions.Read);
        var canUpdateRequests = await scope.HasPermissionAsync(AppResources.Requests, PermissionActions.Update);
        var canReadCompanies = await scope.HasPermissionAsync(AppResources.Companies, PermissionActions.Read);

        var canDeleteCompanies = await scope.HasPermissionAsync(AppResources.Companies, PermissionActions.Delete);
        var canReadUsers = await scope.HasPermissionAsync(AppResources.Users, PermissionActions.Read);
        var canReadRoles = await scope.HasPermissionAsync(AppResources.Roles, PermissionActions.Read);

        canReadRequests.Should().BeTrue();
        canUpdateRequests.Should().BeTrue();
        canReadCompanies.Should().BeTrue();

        canDeleteCompanies.Should().BeFalse("Agent must not be able to delete companies");
        canReadUsers.Should().BeFalse("Agent must not be able to manage internal users");
        canReadRoles.Should().BeFalse("Agent must not be able to manage roles");
    }

    [Fact]
    public async Task Agent_should_have_restricted_allowed_fields_on_dashboard()
    {
        // User 2: Agent 1
        var scope = CreateScopeServiceForUser(2);
        var allowedFields = await scope.GetAllowedFieldsAsync(AppResources.Dashboard);

        allowedFields.Should().NotBeNull();
        allowedFields.Should().Contain("statusDistribution");
        allowedFields.Should().Contain("dailyTrend");
        allowedFields.Should().NotContain("agentPerformance");
        allowedFields.Should().NotContain("totalCompanies");
    }

    [Fact]
    public async Task Expert_should_only_have_access_to_assigned_requests()
    {
        // User 4: Expert
        var scope = CreateScopeServiceForUser(4);

        var canReadAssigned = await scope.HasPermissionAsync(AppResources.AssignedRequests, PermissionActions.Read);
        var canUpdateAssigned = await scope.HasPermissionAsync(AppResources.AssignedRequests, PermissionActions.Update);

        var canReadGeneralRequests = await scope.HasPermissionAsync(AppResources.Requests, PermissionActions.Read);
        var canReadCompanies = await scope.HasPermissionAsync(AppResources.Companies, PermissionActions.Read);
        var canReadUsers = await scope.HasPermissionAsync(AppResources.Users, PermissionActions.Read);

        canReadAssigned.Should().BeTrue();
        canUpdateAssigned.Should().BeTrue();

        canReadGeneralRequests.Should().BeFalse("Expert cannot read general non-assigned requests");
        canReadCompanies.Should().BeFalse("Expert cannot browse companies");
        canReadUsers.Should().BeFalse("Expert cannot browse users");
    }
}
