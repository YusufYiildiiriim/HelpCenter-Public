using FluentAssertions;
using HelpCenter.Domain.Common;
using HelpCenter.Domain.Entities;
using Xunit;

namespace HelpCenter.Application.Tests.Domain;

public class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void BaseEntity_should_initialize_with_default_values()
    {
        var entity = new TestEntity();

        entity.Id.Should().Be(0);
        entity.IsDeleted.Should().BeFalse();
        entity.IsActive.Should().BeTrue();
        entity.UpdatedAt.Should().BeNull();
        entity.RowVersion.Should().NotBeNull();
    }

    [Fact]
    public void BaseEntity_should_support_soft_delete_flag_toggling()
    {
        var entity = new TestEntity { IsDeleted = false };

        entity.MarkAsDeleted();

        entity.IsDeleted.Should().BeTrue();
        entity.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Entities_should_implement_ISoftDelete_and_IAuditable()
    {
        var user = new User();
        var customer = new Customer();
        var request = new CustomerRequest();
        var company = new Company();

        user.Should().BeAssignableTo<ISoftDelete>();
        user.Should().BeAssignableTo<IAuditable>();

        customer.Should().BeAssignableTo<ISoftDelete>();
        customer.Should().BeAssignableTo<IAuditable>();

        request.Should().BeAssignableTo<ISoftDelete>();
        request.Should().BeAssignableTo<IAuditable>();

        company.Should().BeAssignableTo<ISoftDelete>();
        company.Should().BeAssignableTo<IAuditable>();
    }
}
