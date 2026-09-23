
using System.Linq;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace HelpCenter.Application.ServiceRegistration;

public static class AddApplicationService
{
    /// <summary>
    /// The Application layer's assembly. A single reference point for MediatR, FluentValidation,
    /// and AutoMapper scanning (instead of a marker type).
    /// </summary>
    private static readonly Assembly ApplicationAssembly = typeof(AddApplicationService).Assembly;

    public static void AddApplicationServices(this IServiceCollection ServiceCollection)
    {

        ServiceCollection.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(ApplicationAssembly);
            // Order matters: Logging is outermost (so validation errors get logged too),
            // and Validation runs immediately before the handler.
            cfg.AddOpenBehavior(typeof(HelpCenter.Application.Behaviors.LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(HelpCenter.Application.Behaviors.ValidationBehavior<,>));
        });
        ServiceCollection.AddValidatorsFromAssembly(ApplicationAssembly, includeInternalTypes: true);
        ServiceCollection.AddAutoMapper(cfg => { }, ApplicationAssembly);

        ServiceCollection.AddScoped<Features.Roles.Queries.GetUserPermissions.UserPermissionsReader>();

        // Feature- and slice-level rule classes (those ending in *Rules, living in the
        // corresponding Features subfolders) are scanned by convention instead of registering
        // each one manually. Adding a new *Rules class requires no extra line here.
        var ruleTypes = ApplicationAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace != null
                && t.Namespace.StartsWith("HelpCenter.Application.Features.", StringComparison.Ordinal)
                && t.Name.EndsWith("Rules", StringComparison.Ordinal));

        foreach (var ruleType in ruleTypes)
        {
            ServiceCollection.AddScoped(ruleType);
        }
    }
}
