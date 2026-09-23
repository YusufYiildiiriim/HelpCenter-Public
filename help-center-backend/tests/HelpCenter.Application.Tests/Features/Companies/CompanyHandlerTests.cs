using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Companies.Commands.CreateCompany;
using HelpCenter.Application.Features.Companies.Commands.DeleteCompany;
using HelpCenter.Application.Features.Companies.Commands.UpdateCompany;
using HelpCenter.Application.Features.Companies.Queries.GetCompanies;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Companies;

public class CompanyHandlerTests : HandlerTestBase
{
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();

    [Fact]
    public async Task CreateCompany_should_create_company_successfully()
    {
        var handler = new CreateCompanyCommandHandler(Uow, Mapper, _passwordService, new CreateCompanyRules(Uow));
        var command = new CreateCompanyCommand
        {
            Name = "Acme Corp",
            Address = "123 Main St",
            Phone = "555-1234",
            Mail = "info@acme.com"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var company = Db.Companies.FirstOrDefault(c => c.Name == "Acme Corp");
        company.Should().NotBeNull();
        company!.Address.Should().Be("123 Main St");
    }

    [Fact]
    public async Task CreateCompany_should_throw_ConflictException_on_duplicate_name()
    {
        Db.Companies.Add(new Company { Name = "Acme Corp", CreatedAt = DateTime.UtcNow });
        await Db.SaveChangesAsync();

        var handler = new CreateCompanyCommandHandler(Uow, Mapper, _passwordService, new CreateCompanyRules(Uow));
        var command = new CreateCompanyCommand { Name = "Acme Corp" };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CompanyNameAlreadyExistsException>();
    }

    [Fact]
    public async Task UpdateCompany_should_update_company_details()
    {
        var company = new Company { Name = "Old Name", Address = "Old Address", CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(company);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new UpdateCompanyCommandHandler(Uow, Mapper, _passwordService, new UpdateCompanyRules(Uow));
        var command = new UpdateCompanyCommand
        {
            PublicId = company.PublicId,
            Name = "New Name",
            Address = "New Address"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.Companies.Find(company.Id);
        updated!.Name.Should().Be("New Name");
        updated.Address.Should().Be("New Address");
    }

    [Fact]
    public async Task UpdateCompany_should_sync_and_remove_unassigned_modules()
    {
        var mod1 = new Module { Name = "M1", IsActive = true, CreatedAt = DateTime.UtcNow };
        var mod2 = new Module { Name = "M2", IsActive = true, CreatedAt = DateTime.UtcNow };
        var mod3 = new Module { Name = "M3", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Modules.AddRange(mod1, mod2, mod3);
        await Db.SaveChangesAsync();

        var company = new Company { Name = "Sync Company", CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(company);
        await Db.SaveChangesAsync();

        // Company initially has M1 and M2
        Db.CompanyModules.AddRange(
            new CompanyModule { CompanyId = company.Id, ModuleId = mod1.Id, IsActive = true },
            new CompanyModule { CompanyId = company.Id, ModuleId = mod2.Id, IsActive = true }
        );
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new UpdateCompanyCommandHandler(Uow, Mapper, _passwordService, new UpdateCompanyRules(Uow));
        // Admin unchecks M2 and adds M3 -> new modules are [M1, M3]
        var command = new UpdateCompanyCommand
        {
            PublicId = company.PublicId,
            Name = "Sync Company",
            ModuleIds = new List<int> { mod1.Id, mod3.Id }
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var activeModules = Db.CompanyModules.Where(cm => cm.CompanyId == company.Id && !cm.IsDeleted).ToList();
        activeModules.Should().HaveCount(2);
        activeModules.Select(cm => cm.ModuleId).Should().BeEquivalentTo(new[] { mod1.Id, mod3.Id });
        activeModules.Should().NotContain(cm => cm.ModuleId == mod2.Id, "M2 was removed by admin");

        // Unassigned module is soft-deleted, not physically removed — history is preserved
        // and it stays hidden from active queries via the global IsDeleted filter.
        var removedRow = Db.CompanyModules.IgnoreQueryFilters()
            .Single(cm => cm.CompanyId == company.Id && cm.ModuleId == mod2.Id);
        removedRow.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCompany_should_not_throw_ConcurrencyException_when_RowVersion_is_not_sent()
    {
        // Regression coverage for the AdminCompaniesController Base64 fallback bug: a real
        // RowVersion is generated on insert (RowVersionValueGenerator), so it's never empty.
        // A client that omits RowVersion must map to command.RowVersion == null, which skips
        // SetOriginalVersion entirely — not to Array.Empty<byte>(), which would make EF look
        // for a row with an empty RowVersion (never matches) and throw a bogus concurrency error.
        var company = new Company { Name = "No RowVersion Sent Co", Address = "Old Address", CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(company);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var storedRowVersion = Db.Companies.Find(company.Id)!.RowVersion;
        storedRowVersion.Should().NotBeNullOrEmpty("a real RowVersion is always generated on insert");

        var handler = new UpdateCompanyCommandHandler(Uow, Mapper, _passwordService, new UpdateCompanyRules(Uow));
        var command = new UpdateCompanyCommand
        {
            PublicId = company.PublicId,
            Name = "No RowVersion Sent Co",
            Address = "New Address",
            RowVersion = null
        };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().NotThrowAsync<DbUpdateConcurrencyException>();
        var updated = Db.Companies.Find(company.Id);
        updated!.Address.Should().Be("New Address");
    }

    [Fact]
    public async Task UpdateCompany_should_throw_NotFoundException_when_company_does_not_exist()
    {
        var handler = new UpdateCompanyCommandHandler(Uow, Mapper, _passwordService, new UpdateCompanyRules(Uow));
        var command = new UpdateCompanyCommand { PublicId = Guid.NewGuid(), Name = "Non Existent" };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<CompanyNotFoundException>();
    }

    [Fact]
    public async Task DeleteCompany_should_soft_delete_company()
    {
        var company = new Company { Name = "To Delete", CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(company);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new DeleteCompanyCommandHandler(Uow, new DeleteCompanyRules(Uow));
        var result = await handler.Handle(new DeleteCompanyCommand(company.PublicId), CancellationToken.None);

        result.Should().BeTrue();
        var deleted = Db.Companies.Find(company.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCompany_should_throw_NotFoundException_when_company_not_found()
    {
        var handler = new DeleteCompanyCommandHandler(Uow, new DeleteCompanyRules(Uow));

        Func<Task> act = async () => await handler.Handle(new DeleteCompanyCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<CompanyNotFoundException>();
    }

    [Fact]
    public async Task DeleteCompany_should_throw_when_company_has_customer_requests()
    {
        var company = new Company { Name = "Has Requests Co", CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(company);
        await Db.SaveChangesAsync();

        var account = new Account { Email = "req@hasrequests.com", Username = "req_user", FirstName = "R", LastName = "U", Password = "p", CreatedAt = DateTime.UtcNow };
        var customer = new Customer { CompanyId = company.Id, Company = company, Account = account, CreatedAt = DateTime.UtcNow };
        Db.Accounts.Add(account);
        Db.Customers.Add(customer);
        await Db.SaveChangesAsync();

        var request = HelpCenter.Domain.Entities.CustomerRequest.Create(customer.Id, null, null, "SQL Yedekleme Hatası", HelpCenter.Domain.Enums.RequestPriority.High, "R U");
        Db.CustomerRequests.Add(request);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new DeleteCompanyCommandHandler(Uow, new DeleteCompanyRules(Uow));

        Func<Task> act = async () => await handler.Handle(new DeleteCompanyCommand(company.PublicId), CancellationToken.None);

        await act.Should().ThrowAsync<CompanyHasActiveRequestsException>();
        var stillThere = Db.Companies.Find(company.Id);
        stillThere!.IsDeleted.Should().BeFalse("the delete must be blocked before any state changes");
    }

    [Fact]
    public async Task GetCompanies_should_return_paginated_companies()
    {
        for (int i = 1; i <= 5; i++)
        {
            Db.Companies.Add(new Company { Name = $"Company {i}", CreatedAt = DateTime.UtcNow });
        }
        await Db.SaveChangesAsync();

        var handler = new GetCompaniesQueryHandler(Uow, Mapper);
        var response = await handler.Handle(new GetCompaniesQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        response.TotalCount.Should().Be(5);
        response.Items.Should().HaveCount(5);
    }
}
