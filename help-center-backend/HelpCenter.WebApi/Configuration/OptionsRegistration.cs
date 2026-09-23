namespace HelpCenter.WebApi.Configuration;

public static class OptionsRegistration
{
    public static IServiceCollection AddValidatedOptions(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddOptions<JwtOptions>()
            .Bind(cfg.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations().ValidateOnStart();

        services.AddOptions<SmtpOptions>()
            .Bind(cfg.GetSection(SmtpOptions.SectionName))
            .ValidateDataAnnotations().ValidateOnStart();

        return services;
    }
}
