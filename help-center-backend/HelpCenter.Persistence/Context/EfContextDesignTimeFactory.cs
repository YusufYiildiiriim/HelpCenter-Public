using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HelpCenter.Persistence.Context;

/// <summary>
/// Creates the PostgreSQL model for EF tooling without starting the API or reading local
/// environment secrets. Migration commands use this factory and do not connect to the database.
/// </summary>
public sealed class EfContextDesignTimeFactory : IDesignTimeDbContextFactory<EfContext>
{
    public EfContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EfContext>()
            .UseNpgsql("Host=localhost;Database=HelpCenterDesignTime;Username=postgres;Password=design-time-only")
            .Options;

        return new EfContext(options);
    }
}
