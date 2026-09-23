using System.Linq.Expressions;
using FluentAssertions;
using HelpCenter.Application.Features.Faqs.Commands.CreateFaq;
using HelpCenter.Application.Features.Faqs.Commands.DeleteFaq;
using HelpCenter.Application.Features.Faqs.Queries.GetFaqs;
using HelpCenter.Application.Features.Guides.Commands.CreateGuide;
using HelpCenter.Application.Features.Guides.Commands.DeleteGuide;
using HelpCenter.Application.Features.Menu.Queries.GetMenuItems;
using HelpCenter.Application.Features.Organization.Commands.UpdateOrganizationInfo;
using HelpCenter.Application.Features.Organization.Queries.GetOrganizationInfo;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Misc;

public class MiscFeaturesHandlerTests : HandlerTestBase
{
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly IFileService _fileService = Substitute.For<IFileService>();

    [Fact]
    public async Task CreateFaq_should_create_faq_and_invalidate_cache()
    {
        var handler = new CreateFaqCommandHandler(Uow, Mapper, _cacheService, new CreateFaqRules(Uow));
        var command = new CreateFaqCommand
        {
            Title = "How to reset password?",
            Description = "Click forgot password link",
            IsActive = true,
            IsPublic = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.FAQs.Should().ContainSingle(f => f.Title == "How to reset password?");
        _cacheService.Received().InvalidatePrefix(Arg.Any<string>());
    }

    [Fact]
    public async Task DeleteFaq_should_soft_delete_faq()
    {
        var faq = new FAQ { Title = "Q", Description = "A", CreatedAt = DateTime.UtcNow };
        Db.FAQs.Add(faq);
        await Db.SaveChangesAsync();

        var handler = new DeleteFaqCommandHandler(Uow, _cacheService);
        var result = await handler.Handle(new DeleteFaqCommand(faq.Id), CancellationToken.None);

        result.Should().BeTrue();
        Db.FAQs.Find(faq.Id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task CreateGuide_should_create_guide_and_invalidate_cache()
    {
        var handler = new CreateGuideCommandHandler(Uow, Mapper, _fileService, _cacheService, new CreateGuideRules(Uow));
        var command = new CreateGuideCommand
        {
            Title = "Getting Started",
            Description = "Welcome to HelpCenter",
            Module = "General",
            IsActive = true,
            IsPublic = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.Guides.Should().ContainSingle(g => g.Title == "Getting Started");
        _cacheService.Received().InvalidatePrefix(Arg.Any<string>());
    }

    [Fact]
    public async Task CreateFaq_with_duplicate_title_should_throw_FaqTitleAlreadyExistsException()
    {
        var existingFaq = new FAQ { Title = "Duplicate FAQ", Description = "Desc", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.FAQs.Add(existingFaq);
        await Db.SaveChangesAsync();

        var handler = new CreateFaqCommandHandler(Uow, Mapper, _cacheService, new CreateFaqRules(Uow));
        var command = new CreateFaqCommand
        {
            Title = "Duplicate FAQ",
            Description = "New Desc",
            IsActive = true,
            IsPublic = true
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<HelpCenter.Application.Exceptions.FaqTitleAlreadyExistsException>();
    }

    [Fact]
    public async Task CreateGuide_with_duplicate_title_should_throw_GuideTitleAlreadyExistsException()
    {
        var existingGuide = new Guide { Title = "Duplicate Guide", Description = "Desc", Module = "General", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Guides.Add(existingGuide);
        await Db.SaveChangesAsync();

        var handler = new CreateGuideCommandHandler(Uow, Mapper, _fileService, _cacheService, new CreateGuideRules(Uow));
        var command = new CreateGuideCommand
        {
            Title = "Duplicate Guide",
            Description = "New Desc",
            Module = "General",
            IsActive = true,
            IsPublic = true
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<HelpCenter.Application.Exceptions.GuideTitleAlreadyExistsException>();
    }

    [Fact]
    public async Task CreateFaq_with_unassigned_module_to_project_should_throw_ModuleNotAssignedToProjectException()
    {
        var project = new Project { Name = "Test Project", Description = "Desc", IsActive = true, CreatedAt = DateTime.UtcNow };
        var module = new Module { Name = "Test Module", Description = "Desc", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Projects.Add(project);
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();

        var handler = new CreateFaqCommandHandler(Uow, Mapper, _cacheService, new CreateFaqRules(Uow));
        var command = new CreateFaqCommand
        {
            Title = "FAQ with unassigned module",
            Description = "Answer",
            ProjectId = project.Id,
            ModuleId = module.Id,
            IsActive = true
        };

        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<HelpCenter.Application.Exceptions.ModuleNotAssignedToProjectException>();
    }

    [Fact]
    public async Task CreateFaq_with_valid_project_and_module_should_succeed()
    {
        var project = new Project { Name = "Portal Project", Description = "Desc", IsActive = true, CreatedAt = DateTime.UtcNow };
        var module = new Module { Name = "Billing Module", Description = "Desc", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Projects.Add(project);
        Db.Modules.Add(module);
        await Db.SaveChangesAsync();

        Db.ProjectModules.Add(new ProjectModule { ProjectId = project.Id, ModuleId = module.Id, IsActive = true, CreatedAt = DateTime.UtcNow });
        await Db.SaveChangesAsync();

        var handler = new CreateFaqCommandHandler(Uow, Mapper, _cacheService, new CreateFaqRules(Uow));
        var command = new CreateFaqCommand
        {
            Title = "How to pay invoice?",
            Description = "Go to billing tab",
            ProjectId = project.Id,
            ModuleId = module.Id,
            IsActive = true
        };

        var result = await handler.Handle(command, CancellationToken.None);
        result.Should().BeTrue();

        var created = Db.FAQs.FirstOrDefault(f => f.Title == "How to pay invoice?");
        created.Should().NotBeNull();
        created!.ProjectId.Should().Be(project.Id);
        created.ModuleId.Should().Be(module.Id);
    }

    [Fact]
    public async Task UpdateOrganizationInfo_should_update_organization()
    {
        var org = OrganizationInfo.Create("Old HQ", phone: "123", email: "old@test.com");
        Db.OrganizationInfos.Add(org);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var auditLog = Substitute.For<IAuditLogWriter>();
        var handler = new UpdateOrganizationInfoCommandHandler(Uow, _cacheService, auditLog, _fileService);
        var command = new UpdateOrganizationInfoCommand
        {
            Id = org.Id,
            OrganizationName = "Support HQ",
            Email = "support@example.com"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.OrganizationInfos.Find(org.Id)!.OrganizationName.Should().Be("Support HQ");
    }

    [Fact]
    public async Task UpdateOrganizationInfo_should_delete_old_logo_only_after_save_succeeds()
    {
        var org = OrganizationInfo.Create("Old HQ", phone: "123", email: "old@test.com");
        org.SetLogo("/uploads/Organization/old-logo.png");
        Db.OrganizationInfos.Add(org);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var auditLog = Substitute.For<IAuditLogWriter>();
        var handler = new UpdateOrganizationInfoCommandHandler(Uow, _cacheService, auditLog, _fileService);
        var command = new UpdateOrganizationInfoCommand
        {
            Id = org.Id,
            OrganizationName = "Support HQ",
            LogoUrl = "/uploads/Organization/new-logo.png"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.OrganizationInfos.Find(org.Id)!.LogoUrl.Should().Be("/uploads/Organization/new-logo.png");
        _fileService.Received(1).DeleteFile("/uploads/Organization/old-logo.png");
    }

    [Fact]
    public async Task UpdateOrganizationInfo_should_not_delete_old_logo_when_SaveAsync_fails()
    {
        var org = OrganizationInfo.Create("Old HQ", phone: "123", email: "old@test.com");
        org.SetLogo("/uploads/Organization/old-logo.png");

        var repository = Substitute.For<IGenericRepository<OrganizationInfo>>();
        repository.FirstOrDefaultAsync(
                Arg.Any<Expression<Func<OrganizationInfo, bool>>>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<Expression<Func<OrganizationInfo, object>>[]>())
            .Returns(org);
        repository.UpdateAsync(org, Arg.Any<CancellationToken>()).Returns(true);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.Repository<OrganizationInfo>().Returns(repository);
        unitOfWork.SaveAsync(Arg.Any<CancellationToken>())
            .Throws(new DbUpdateConcurrencyException("Kayıt başka biri tarafından güncellendi."));

        var auditLog = Substitute.For<IAuditLogWriter>();
        var handler = new UpdateOrganizationInfoCommandHandler(unitOfWork, _cacheService, auditLog, _fileService);
        var command = new UpdateOrganizationInfoCommand
        {
            Id = org.Id,
            OrganizationName = "Support HQ",
            LogoUrl = "/uploads/Organization/new-logo.png",
            RowVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
        };

        await FluentActions.Awaiting(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<DbUpdateConcurrencyException>();

        // Since SaveAsync failed, the old logo file must never be touched — the DB still
        // points to the old LogoUrl, so the file must remain on disk.
        _fileService.DidNotReceive().DeleteFile(Arg.Any<string>());
    }

    [Fact]
    public async Task GetMenuItems_should_return_active_menu_items()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(1);

        var role = new Role { Name = "AdminRole", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Roles.Add(role);
        var userRole = new UserRole { UserId = 1, Role = role, CreatedAt = DateTime.UtcNow };
        Db.UserRoles.Add(userRole);

        var perm = new RolePermission { Role = role, ResourceKey = "Dashboard", CreatedAt = DateTime.UtcNow };
        perm.Actions.Add(new RolePermissionAction { Action = "Read", CreatedAt = DateTime.UtcNow });
        Db.RolePermissions.Add(perm);

        Db.MenuItems.AddRange(
            new MenuItem { Label = "Dashboard", Route = "/dashboard", ResourceKey = "Dashboard", IsActive = true, Order = 1, CreatedAt = DateTime.UtcNow },
            new MenuItem { Label = "Disabled", Route = "/disabled", ResourceKey = "Dashboard", IsActive = false, Order = 2, CreatedAt = DateTime.UtcNow }
        );
        await Db.SaveChangesAsync();

        var handler = new GetMenuItemsQueryHandler(Uow, userContext);
        var result = await handler.Handle(new GetMenuItemsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Label.Should().Be("Dashboard");
    }
}
