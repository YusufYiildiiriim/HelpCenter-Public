using HelpCenter.Application.Interfaces;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HelpCenter.Persistence.ServiceRegistration
{
    public static class AddPersistenceService
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddDbContext<EfContext>(options =>
              options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
              sqlOptions => sqlOptions.EnableRetryOnFailure()));


            services.AddHttpContextAccessor();
            services.AddScoped<IUnitOfWork, HelpCenter.Persistence.Repositories.UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(HelpCenter.Persistence.Repositories.GenericRepository<>));
            services.AddScoped<IAuditLogWriter, HelpCenter.Persistence.Services.AuditLogWriter>();
            services.AddScoped<IStatisticsReadRepository, HelpCenter.Persistence.Repositories.StatisticsReadRepository>();
            services.AddScoped<IRoleQueryRepository, HelpCenter.Persistence.Repositories.RoleQueryRepository>();
            services.AddScoped<IConversationReadService, HelpCenter.Persistence.Services.ConversationReadService>();
            services.AddScoped<IRolePermissionSyncService, HelpCenter.Persistence.Services.RolePermissionSyncService>();

            // Only runs in Development/Staging, adds sample/demo data idempotently
            // (see SampleDataSeedHostedService — never leaks into production).
            services.AddHostedService<HelpCenter.Persistence.Services.SampleDataSeedHostedService>();

            return services;
        }
    }
}
