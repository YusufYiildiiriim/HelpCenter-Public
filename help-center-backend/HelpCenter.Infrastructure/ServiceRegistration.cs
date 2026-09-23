using HelpCenter.Application.Interfaces;
using HelpCenter.Infrastructure.Services;
using HelpCenter.Infrastructure.Services.Cache;
using HelpCenter.Infrastructure.Services.Email;
using HelpCenter.Infrastructure.Services.Jwt;
using Microsoft.Extensions.DependencyInjection;

namespace HelpCenter.Infrastructure;

public static class ServiceRegistration
{
    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<IGenerateJwtToken, GenerateJwtToken>();
        services.AddScoped<IAuthTokenService, AuthTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IFileService, FileService>();

        // Mail: EmailService (Scoped - actual SMTP sender)
        services.AddScoped<IEmailService, EmailService>();

        // Mail background queue
        services.AddSingleton<EmailQueue>();
        services.AddScoped<IEmailDispatcher, EmailDispatcher>();
        services.AddHostedService<EmailBackgroundService>();
    }
}
