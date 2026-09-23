using AutoMapper;
using HelpCenter.Application.Interfaces;
using HelpCenter.Application.ServiceRegistration;
using HelpCenter.Persistence.Context;
using HelpCenter.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Tests.Common;

public abstract class HandlerTestBase : IDisposable
{
    private static readonly IMapper SharedMapper = CreateSharedMapper();

    private static IMapper CreateSharedMapper()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddLogging();
        services.AddApplicationServices();
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    protected EfContext Db { get; }
    protected IUnitOfWork Uow { get; }
    protected static IMapper Mapper => SharedMapper;

    protected HandlerTestBase()
    {
        var options = new DbContextOptionsBuilder<EfContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Db = new EfContext(options);

        Uow = new UnitOfWork(Db);
    }

    public void Dispose()
    {
        Db.Dispose();
        GC.SuppressFinalize(this);
    }
}
