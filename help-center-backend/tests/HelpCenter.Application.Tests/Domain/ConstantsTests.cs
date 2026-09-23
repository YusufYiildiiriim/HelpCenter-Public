using FluentAssertions;
using HelpCenter.Domain.Constants;
using Xunit;

namespace HelpCenter.Application.Tests.Domain;

public class ConstantsTests
{
    [Fact]
    public void AppResources_should_contain_core_resources()
    {
        AppResources.Dashboard.Should().Be("Dashboard");
        AppResources.Requests.Should().Be("Requests");
        AppResources.Companies.Should().Be("Companies");
        AppResources.Users.Should().Be("Users");
        AppResources.Roles.Should().Be("Roles");
        AppResources.Modules.Should().Be("Modules");
        AppResources.Projects.Should().Be("Projects");
        AppResources.Customers.Should().Be("Customers");
    }

    [Fact]
    public void DashboardWidgets_All_should_contain_expected_widgets()
    {
        DashboardWidgets.All.Should().NotBeEmpty();
        DashboardWidgets.All.Should().Contain(DashboardWidgets.TotalRequests);
        DashboardWidgets.All.Should().Contain(DashboardWidgets.ActiveRequests);
        DashboardWidgets.All.Should().Contain(DashboardWidgets.CompletedRequests);
        DashboardWidgets.All.Should().Contain(DashboardWidgets.TicketFlow);
    }

    [Fact]
    public void PermissionActions_should_define_standard_actions()
    {
        PermissionActions.Read.Should().Be("Read");
        PermissionActions.Create.Should().Be("Create");
        PermissionActions.Update.Should().Be("Update");
        PermissionActions.Delete.Should().Be("Delete");
        PermissionActions.Assign.Should().Be("Assign");
        PermissionActions.ChangeStatus.Should().Be("ChangeStatus");
    }
}
