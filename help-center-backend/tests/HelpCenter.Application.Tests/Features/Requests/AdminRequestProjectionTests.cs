using FluentAssertions;
using HelpCenter.Application.Features.Requests.Queries.GetAdminRequestById;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using HelpCenter.Domain.Enums;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Requests;

public class AdminRequestProjectionTests : HandlerTestBase
{
    [Fact]
    public async Task Detail_projection_returns_assigned_agent_and_expert_names_from_accounts()
    {
        var company = new Company { Name = "Projection Co" };
        var customer = new Customer
        {
            Company = company,
            Account = new Account
            {
                Email = "customer@projection.test",
                Username = "projection.customer",
                FirstName = "Test",
                LastName = "Customer",
                Password = "hash"
            }
        };
        var assigned = new User
        {
            Account = new Account
            {
                Email = "agent@projection.test",
                Username = "projection.agent",
                FirstName = "Assigned",
                LastName = "Agent",
                Password = "hash"
            }
        };
        var expert = new User
        {
            Account = new Account
            {
                Email = "expert@projection.test",
                Username = "projection.expert",
                FirstName = "Current",
                LastName = "Expert",
                Password = "hash"
            }
        };

        Db.Companies.Add(company);
        Db.Customers.Add(customer);
        Db.Users.AddRange(assigned, expert);
        Db.CustomerRequestStatuses.Add(new CustomerRequestStatus
        {
            Id = Domain.Constants.RequestStatusConstants.UnderTechnicalReview,
            Name = "Teknik İncelemede"
        });
        await Db.SaveChangesAsync();

        var request = CustomerRequest.Create(customer.Id, null, null, "Projection test", RequestPriority.Medium);
        request.StatusId = Domain.Constants.RequestStatusConstants.UnderTechnicalReview;
        request.AssignedUserId = assigned.Id;
        request.CurrentExpertId = expert.Id;
        Db.CustomerRequests.Add(request);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var userContext = Substitute.For<HelpCenter.Application.Interfaces.IUserContext>();
        var handler = new GetAdminRequestByIdQueryHandler(Uow, Mapper, userContext);

        var result = await handler.Handle(new GetAdminRequestByIdQuery(request.PublicId), default);

        result.AssignedUserName.Should().Be("Assigned Agent");
        result.CurrentExpertName.Should().Be("Current Expert");
    }
}
