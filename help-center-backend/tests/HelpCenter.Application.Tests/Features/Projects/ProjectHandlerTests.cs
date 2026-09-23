using FluentAssertions;
using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Projects.Commands.AssignUserToProject;
using HelpCenter.Application.Features.Projects.Commands.CreateProject;
using HelpCenter.Application.Features.Projects.Commands.DeleteProject;
using HelpCenter.Application.Features.Projects.Commands.RemoveUserFromProject;
using HelpCenter.Application.Features.Projects.Commands.UpdateProject;
using HelpCenter.Application.Features.Projects.Queries.GetProjects;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Projects;

public class ProjectHandlerTests : HandlerTestBase
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
    public async Task CreateProject_should_create_project_and_invalidate_caches()
    {
        var handler = new CreateProjectCommandHandler(Uow, _cacheService, new CreateProjectRules(Uow));
        var command = new CreateProjectCommand
        {
            Name = "Core Banking",
            Description = "Main banking platform",
            IsActive = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.Projects.Should().ContainSingle(p => p.Name == "Core Banking");
        _cacheService.Received().InvalidatePrefix(Arg.Any<string>());
    }

    [Fact]
    public async Task CreateProject_should_throw_ConflictException_on_duplicate_name()
    {
        Db.Projects.Add(new Project { Name = "Core Banking", CreatedAt = DateTime.UtcNow });
        await Db.SaveChangesAsync();

        var handler = new CreateProjectCommandHandler(Uow, _cacheService, new CreateProjectRules(Uow));
        var command = new CreateProjectCommand { Name = "Core Banking" };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ProjectNameAlreadyExistsException>();
    }

    [Fact]
    public async Task UpdateProject_should_update_project_details()
    {
        var project = new Project { Name = "Old Project", Description = "Old", IsActive = true, CreatedAt = DateTime.UtcNow };
        Db.Projects.Add(project);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new UpdateProjectCommandHandler(Uow, _cacheService, new UpdateProjectRules(Uow));
        var command = new UpdateProjectCommand
        {
            PublicId = project.PublicId,
            Name = "Updated Project",
            Description = "Updated",
            IsActive = true
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        var updated = Db.Projects.Find(project.Id);
        updated!.Name.Should().Be("Updated Project");
    }

    [Fact]
    public async Task DeleteProject_should_soft_delete_project()
    {
        var project = new Project { Name = "To Delete", CreatedAt = DateTime.UtcNow };
        Db.Projects.Add(project);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new DeleteProjectCommandHandler(
            Uow, Microsoft.Extensions.Logging.Abstractions.NullLogger<DeleteProjectCommandHandler>.Instance,
            _cacheService);
        var result = await handler.Handle(new DeleteProjectCommand(project.PublicId), CancellationToken.None);

        result.Should().BeTrue();
        Db.Projects.Find(project.Id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task AssignUserToProject_should_assign_user()
    {
        var project = new Project { Name = "E-Commerce", CreatedAt = DateTime.UtcNow };
        Db.Projects.Add(project);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var user = SeedUser("projectuser@example.com", "project.user");

        var handler = new AssignUserToProjectCommandHandler(Uow);
        var command = new AssignUserToProjectCommand { ProjectId = project.Id, UserId = user.Id };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.UserProjects.Should().ContainSingle(up => up.ProjectId == project.Id && up.UserId == user.Id);
    }

    [Fact]
    public async Task AssignUserToProject_should_throw_UserNotFoundException_when_user_does_not_exist()
    {
        var project = new Project { Name = "E-Commerce", CreatedAt = DateTime.UtcNow };
        Db.Projects.Add(project);
        await Db.SaveChangesAsync();
        Db.ChangeTracker.Clear();

        var handler = new AssignUserToProjectCommandHandler(Uow);
        var command = new AssignUserToProjectCommand { ProjectId = project.Id, UserId = 99999 };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
        Db.UserProjects.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveUserFromProject_should_remove_assignment()
    {
        var project = new Project { Name = "Mobile App", CreatedAt = DateTime.UtcNow };
        Db.Projects.Add(project);
        var userProject = new UserProject { Project = project, UserId = 3, CreatedAt = DateTime.UtcNow };
        Db.UserProjects.Add(userProject);
        await Db.SaveChangesAsync();

        var handler = new RemoveUserFromProjectCommandHandler(Uow);
        var command = new RemoveUserFromProjectCommand(userProject.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        Db.UserProjects.Find(userProject.Id)!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetProjects_should_return_paginated_projects()
    {
        for (int i = 1; i <= 3; i++)
        {
            Db.Projects.Add(new Project { Name = $"Project {i}", CreatedAt = DateTime.UtcNow });
        }
        await Db.SaveChangesAsync();

        var handler = new GetProjectsQueryHandler(Uow, Mapper, Microsoft.Extensions.Logging.Abstractions.NullLogger<GetProjectsQueryHandler>.Instance);
        var result = await handler.Handle(new GetProjectsQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }
}
