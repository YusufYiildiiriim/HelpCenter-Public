using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Features.Modules.Queries.GetAvailableExpertsForModule;
using HelpCenter.Application.Features.Projects.Queries.GetAvailableUsersForProject;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.ServiceRegistration;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Repositories;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HelpCenter.Application.Tests.Features.Projects;

public sealed class UserLookupTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly EfContext _db;
    private readonly ServiceProvider _services;

    public UserLookupTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _db = new EfContext(new DbContextOptionsBuilder<EfContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();
        services.AddScoped<IUnitOfWork>(_ => new UnitOfWork(_db));
        _services = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Both_registered_handlers_translate_the_shared_lookup_and_exclude_inactive_or_deleted_users()
    {
        var active = User.Register("active@example.test", "Çağrı", "Öztürk", "hash", "active");
        var inactive = User.Register("inactive@example.test", "Inactive", "User", "hash", "inactive");
        inactive.IsActive = false;
        var deleted = User.Register("deleted@example.test", "Deleted", "User", "hash", "deleted");
        deleted.MarkAsDeleted();
        _db.Users.AddRange(active, inactive, deleted);
        await _db.SaveChangesAsync();
        _db.ChangeTracker.Clear();
        using var scope = _services.CreateScope();
        var projects = scope.ServiceProvider.GetRequiredService<IRequestHandler<GetAvailableUsersForProjectQuery, List<LookupItem>>>();
        var modules = scope.ServiceProvider.GetRequiredService<IRequestHandler<GetAvailableExpertsForModuleQuery, List<LookupItem>>>();

        var projectUsers = await projects.Handle(new GetAvailableUsersForProjectQuery(), CancellationToken.None);
        var moduleExperts = await modules.Handle(new GetAvailableExpertsForModuleQuery(), CancellationToken.None);

        Assert.Contains(new LookupItem(active.Id, "Çağrı Öztürk"), projectUsers);
        Assert.DoesNotContain(projectUsers, item => item.Id == inactive.Id || item.Id == deleted.Id);
        Assert.Equal(projectUsers.OrderBy(item => item.Id), moduleExperts.OrderBy(item => item.Id));
    }

    [Fact]
    public async Task Both_handlers_propagate_cancellation_to_the_database()
    {
        using var scope = _services.CreateScope();
        var projects = scope.ServiceProvider.GetRequiredService<IRequestHandler<GetAvailableUsersForProjectQuery, List<LookupItem>>>();
        var modules = scope.ServiceProvider.GetRequiredService<IRequestHandler<GetAvailableExpertsForModuleQuery, List<LookupItem>>>();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => projects.Handle(new GetAvailableUsersForProjectQuery(), cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => modules.Handle(new GetAvailableExpertsForModuleQuery(), cancellation.Token));
    }

    public void Dispose()
    {
        _services.Dispose();
        _db.Dispose();
        _connection.Dispose();
    }
}
