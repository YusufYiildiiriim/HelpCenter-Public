using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Customers.Commands.CreateCustomer;
using HelpCenter.Application.Features.Customers.Commands.DeleteCustomer;
using HelpCenter.Application.Features.Customers.Commands.UpdateCustomer;
using HelpCenter.Application.Features.Customers.Commands.UpdateProfile;
using HelpCenter.Application.Features.Customers.Queries.GetCustomerById;
using HelpCenter.Application.Features.Customers.Queries.GetCustomers;
using HelpCenter.Application.Features.Customers.Queries.GetProfile;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Customers;

public class CustomerHandlerTests : HandlerTestBase
{
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IAuditLogWriter _auditLog = Substitute.For<IAuditLogWriter>();

    private Customer SeedCustomer(string email, string username)
    {
        var company = new Company { Name = "CustomerCo_" + Guid.NewGuid().ToString("N")[..8], CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(company);
        Db.SaveChanges();

        var account = new Account
        {
            Email = email,
            Username = username,
            FirstName = "John",
            LastName = "Doe",
            Password = "hashed_password",
            CreatedAt = DateTime.UtcNow
        };
        var customer = new Customer
        {
            CompanyId = company.Id,
            Company = company,
            Account = account,
            CreatedAt = DateTime.UtcNow
        };
        Db.Accounts.Add(account);
        Db.Customers.Add(customer);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
        return customer;
    }

    [Fact]
    public async Task CreateCustomer_should_create_customer_and_account()
    {
        _passwordService.HashPassword("Pass123!").Returns("pwd_hash");

        var handler = new CreateCustomerCommandHandler(Uow, _passwordService, _auditLog);
        var command = new CreateCustomerCommand
        {
            CompanyId = 1,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            PhoneNumber = "123456",
            Password = "Pass123!",
            Username = "alice.smith"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var customer = Db.Customers.FirstOrDefault(c => c.Account.Email == "alice@example.com");
        customer.Should().NotBeNull();
        customer!.Account.FirstName.Should().Be("Alice");
        await _auditLog.Received(1).WriteAsync("CustomerCreated", "Customer", null, Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCustomer_should_return_false_when_email_already_exists()
    {
        SeedCustomer("duplicate@example.com", "dup.user");

        var handler = new CreateCustomerCommandHandler(Uow, _passwordService, _auditLog);
        var command = new CreateCustomerCommand
        {
            CompanyId = 1,
            FirstName = "Dup",
            LastName = "User",
            Email = "duplicate@example.com",
            Password = "pwd"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateCustomer_should_update_customer_and_account_info()
    {
        var customer = SeedCustomer("old@example.com", "old.user");

        var handler = new UpdateCustomerCommandHandler(Uow, _auditLog);
        var command = new UpdateCustomerCommand
        {
            PublicId = customer.PublicId,
            FirstName = "UpdatedFirst",
            LastName = "UpdatedLast",
            Email = "updated@example.com",
            PhoneNumber = "999999",
            IsActive = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.Customers.Find(customer.Id);
        updated!.FirstName.Should().Be("UpdatedFirst");
        updated.Email.Should().Be("updated@example.com");
        await _auditLog.Received(1).WriteDiffAsync("CustomerUpdated", "Customer", customer.Id, Arg.Any<object>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteCustomer_should_soft_delete_customer()
    {
        var customer = SeedCustomer("delete@example.com", "delete.user");

        var handler = new DeleteCustomerCommandHandler(Uow, _auditLog);
        var result = await handler.Handle(new DeleteCustomerCommand(customer.PublicId), CancellationToken.None);

        result.Should().BeTrue();
        var deleted = Db.Customers.Find(customer.Id);
        deleted!.IsDeleted.Should().BeTrue();
        await _auditLog.Received(1).WriteDiffAsync("CustomerDeleted", "Customer", customer.Id, Arg.Any<object>(), Arg.Any<object>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCustomerById_should_return_customer_detail_dto()
    {
        var customer = SeedCustomer("get@example.com", "get.user");

        var handler = new GetCustomerByIdQueryHandler(Uow, Mapper);
        var result = await handler.Handle(new GetCustomerByIdQuery(customer.PublicId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.PublicId.Should().Be(customer.PublicId);
    }

    [Fact]
    public async Task GetProfile_should_return_profile_dto()
    {
        var customer = SeedCustomer("profile@example.com", "profile.user");

        var handler = new GetProfileQueryHandler(Uow, Mapper);
        var result = await handler.Handle(new GetProfileQuery(customer.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result.PublicId.Should().Be(customer.PublicId);
    }
}
