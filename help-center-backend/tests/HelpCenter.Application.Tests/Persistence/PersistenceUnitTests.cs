using FluentAssertions;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.Tests.Common;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Repositories;
using HelpCenter.Persistence.Services;
using HelpCenter.Persistence.ValueGeneration;
using NSubstitute;
using Xunit;

namespace HelpCenter.Application.Tests.Persistence;

public class PersistenceUnitTests : HandlerTestBase
{
    [Fact]
    public void UnitOfWork_Repository_should_cache_instances()
    {
        var repo1 = Uow.Repository<Role>();
        var repo2 = Uow.Repository<Role>();

        repo1.Should().BeSameAs(repo2);
    }

    [Fact]
    public void RowVersionValueGenerator_should_generate_16_bytes()
    {
        var generator = new RowVersionValueGenerator();
        generator.GeneratesTemporaryValues.Should().BeFalse();

        var val1 = generator.Next(null!);
        var val2 = generator.Next(null!);

        val1.Should().NotBeNull();
        val1.Length.Should().Be(16);
        val1.Should().NotEqual(val2);
    }

    [Fact]
    public void RefreshToken_customer_fk_should_not_cascade()
    {
        var relationship = Db.Model
            .FindEntityType(typeof(RefreshToken))!
            .FindNavigation(nameof(RefreshToken.Customer))!
            .ForeignKey;

        relationship.DeleteBehavior.Should().Be(Microsoft.EntityFrameworkCore.DeleteBehavior.Restrict);
    }

    [Fact]
    public async Task DataScopeService_HasPermissionAsync_should_return_true_when_user_has_action()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(1);

        var role = new Role { Name = "AdminRole", CreatedAt = DateTime.UtcNow };
        Db.Roles.Add(role);
        var userRole = new UserRole { UserId = 1, Role = role, CreatedAt = DateTime.UtcNow };
        Db.UserRoles.Add(userRole);

        var rolePerm = new RolePermission { Role = role, ResourceKey = "Users", CreatedAt = DateTime.UtcNow };
        rolePerm.Actions.Add(new RolePermissionAction { Action = "Read", CreatedAt = DateTime.UtcNow });
        Db.RolePermissions.Add(rolePerm);
        await Db.SaveChangesAsync();

        var service = new DataScopeService(Uow, userContext);

        var hasRead = await service.HasPermissionAsync("Users", "Read");
        var hasDelete = await service.HasPermissionAsync("Users", "Delete");

        hasRead.Should().BeTrue();
        hasDelete.Should().BeFalse();
    }

    [Fact]
    public async Task DataScopeService_GetAllowedFieldsAsync_should_merge_allowed_fields()
    {
        var userContext = Substitute.For<IUserContext>();
        userContext.UserId.Returns(2);

        var role = new Role { Name = "EditorRole", CreatedAt = DateTime.UtcNow };
        Db.Roles.Add(role);
        Db.UserRoles.Add(new UserRole { UserId = 2, Role = role, CreatedAt = DateTime.UtcNow });

        var rolePerm = new RolePermission
        {
            Role = role,
            ResourceKey = "Requests",
            AllowedFieldsJson = "[\"Title\", \"Description\"]",
            CreatedAt = DateTime.UtcNow
        };
        Db.RolePermissions.Add(rolePerm);
        await Db.SaveChangesAsync();

        var service = new DataScopeService(Uow, userContext);

        var fields = await service.GetAllowedFieldsAsync("Requests");

        fields.Should().NotBeNull();
        fields.Should().Contain("Title");
        fields.Should().Contain("Description");
        fields.Should().NotContain("Priority");
    }
}
