using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Modules.Commands.AssignExpertToModule;
using HelpCenter.Application.Features.Modules.Commands.CreateModule;
using HelpCenter.Application.Features.Modules.Commands.DeleteModule;
using HelpCenter.Application.Features.Modules.Commands.RemoveExpertFromModule;
using HelpCenter.Application.Features.Modules.Commands.UpdateModule;
using HelpCenter.Application.Features.Modules.Queries.GetCustomerModules;
using HelpCenter.Application.Features.Modules.Queries.GetExpertsByModuleId;
using HelpCenter.Application.Features.Modules.Queries.GetModules;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Modules;

public class ModuleHandlerTests : HandlerTestBase
{
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();

    private User SeedUser(string email, string username)
    {
        var account = new Account
        {
            Email = email,
            Username = username,
            FirstName = "First",
            LastName = "Last",
            Password = "hashed_pass",
            CreatedAt = DateTime.UtcNow
        };
        var user = new User
        {
            Account = account,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        Db.Accounts.Add(account);
        Db.Users.Add(user);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
        return user;
    }

    [Fact]
    public async Task CreateModule_should_create_module_and_invalidate_cache()
    {
        var handler = new CreateModuleCommandHandler(Uow, _cacheService, new CreateModuleRules(Uow));
        var command = new CreateModuleCommand
        {
            Name = "Billing Module",
            Description = "Handles billing",
            IsActive = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.Modules.Should().ContainSingle(m => m.Name == "Billing Module");
        _cacheService.Received().InvalidatePrefix(Arg.Any<string>());
    }

    [Fact]
    public async Task CreateModule_should_throw_ConflictException_on_duplicate_name()
    {
        Db.Modules.Add(new Module { Name = "Billing Module", CreatedAt = DateTime.UtcNow });
        await Db.SaveChangesAsync();

        var handler = new CreateModuleCommandHandler(Uow, _cacheService, new CreateModuleRules(Uow));
        var command = new CreateModuleCommand { Name = "Billing Module" };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ModuleNameAlreadyExistsException>();
    }

    [Fact]
    public async Task UpdateModule_should_update_module_details()
    {
        var module = new Module { Name = "Old Module", Description = "Old", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new UpdateModuleCommandHandler(Uow, _cacheService, new UpdateModuleRules(Uow));
        var command = new UpdateModuleCommand
        {
            PublicId = module.PublicId,
            Name = "New Module",
            Description = "New",
            IsActive = false
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.Modules.Find(module.Id);
        updated!.Name.Should().Be("New Module");
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteModule_should_soft_delete_module()
    {
        var module = new Module { Name = "To Delete", CreatedAt = DateTime.UtcNow };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new DeleteModuleCommandHandler(
            Uow, Microsoft.Extensions.Logging.Abstractions.NullLogger<DeleteModuleCommandHandler>.Instance,
            _cacheService);
        var result = await handler.Handle(new DeleteModuleCommand(module.PublicId), CancellationToken.None);

        result.Should().BeTrue();
        var deleted = Db.Modules.Find(module.Id);
        deleted!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteModule_should_throw_ModuleLockedException_when_module_is_locked()
    {
        var module = new Module { Name = "Locked Module", CreatedAt = DateTime.UtcNow, IsLocked = true };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new DeleteModuleCommandHandler(
            Uow, Microsoft.Extensions.Logging.Abstractions.NullLogger<DeleteModuleCommandHandler>.Instance,
            _cacheService);

        Func<Task> act = async () => await handler.Handle(new DeleteModuleCommand(module.PublicId), CancellationToken.None);

        await act.Should().ThrowAsync<ModuleLockedException>();
        Db.Modules.Find(module.Id)!.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task AssignExpertToModule_should_assign_user_as_expert()
    {
        var module = new Module { Name = "Auth Module", CreatedAt = DateTime.UtcNow };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var user = SeedUser("expert@example.com", "expert.user");

        var handler = new AssignExpertToModuleCommandHandler(Uow);
        var command = new AssignExpertToModuleCommand { ModuleId = module.Id, UserId = user.Id };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.ModuleExperts.Should().ContainSingle(me => me.ModuleId == module.Id && me.UserId == user.Id);
    }

    [Fact]
    public async Task AssignExpertToModule_should_throw_UserNotFoundException_when_user_does_not_exist()
    {
        var module = new Module { Name = "Auth Module", CreatedAt = DateTime.UtcNow };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new AssignExpertToModuleCommandHandler(Uow);
        var command = new AssignExpertToModuleCommand { ModuleId = module.Id, UserId = 99999 };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
        Db.ModuleExperts.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateModule_should_throw_UserNotFoundException_when_expert_user_does_not_exist()
    {
        var handler = new CreateModuleCommandHandler(Uow, _cacheService, new CreateModuleRules(Uow));
        var command = new CreateModuleCommand
        {
            Name = "Support Module",
            Description = "Handles support",
            IsActive = true,
            ExpertUserIds = new List<int> { 99999 }
        };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
        Db.Modules.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateModule_should_throw_UserNotFoundException_when_expert_user_does_not_exist()
    {
        var module = new Module { Name = "Old Module", Description = "Old", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new UpdateModuleCommandHandler(Uow, _cacheService, new UpdateModuleRules(Uow));
        var command = new UpdateModuleCommand
        {
            PublicId = module.PublicId,
            Name = "New Module",
            Description = "New",
            IsActive = true,
            ExpertUserIds = new List<int> { 99999 }
        };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task RemoveExpertFromModule_should_remove_expert_assignment()
    {
        var module = new Module { Name = "Reporting Module", CreatedAt = DateTime.UtcNow };
        Db.Modules.Add(module);
        var me = new ModuleExpert { Module = module, UserId = 5, CreatedAt = DateTime.UtcNow };
        Db.ModuleExperts.Add(me);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new RemoveExpertFromModuleCommandHandler(Uow);
        var command = new RemoveExpertFromModuleCommand(me.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.ModuleExperts.Find(me.Id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetCustomerModules_should_return_only_company_and_project_assigned_modules()
    {
        var mod1 = new Module { Name = "Muhasebe", IsActive = true, CreatedAt = DateTime.UtcNow };
        var mod2 = new Module { Name = "Stok", IsActive = true, CreatedAt = DateTime.UtcNow };
        var mod3 = new Module { Name = "IK", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Modules.AddRange(mod1, mod2, mod3);
        await Db.SaveChangesAsync();

        var project = Project.Create("ERP Projesi", "Test", true);
        project.SyncModules(new[] { mod1.Id, mod2.Id }); // Project has mod1, mod2
        Db.Projects.Add(project);
        await Db.SaveChangesAsync();

        var company = new Company { Name = "Acme", ProjectId = project.Id, CreatedAt = DateTime.UtcNow };
        Db.Companies.Add(company);
        await Db.SaveChangesAsync();

        // Company only licensed for mod1
        Db.CompanyModules.Add(new CompanyModule { CompanyId = company.Id, ModuleId = mod1.Id, IsActive = true });
        await Db.SaveChangesAsync();

        var account = new Account { Email = "cust@acme.com", Username = "cust_acme", FirstName = "C", LastName = "A", Password = "p", CreatedAt = DateTime.UtcNow };
        var customer = new Customer { CompanyId = company.Id, Company = company, Account = account, CreatedAt = DateTime.UtcNow };
        Db.Accounts.Add(account);
        Db.Customers.Add(customer);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new GetCustomerModulesQueryHandler(Uow, Mapper);
        var result = await handler.Handle(new GetCustomerModulesQuery { CustomerId = customer.Id, CompanyId = company.Id }, CancellationToken.None);

        result.Should().ContainSingle(m => m.Id == mod1.Id);
        result.Should().NotContain(m => m.Id == mod2.Id, "Company only has mod1 licensed");
        result.Should().NotContain(m => m.Id == mod3.Id, "mod3 is not in company or project");
    }
}
