using HelpCenter.WebApi.Errors;

namespace HelpCenter.WebApi.ServiceRegistration;

public static class ExceptionHandlingServiceRegistration
{
    public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}
