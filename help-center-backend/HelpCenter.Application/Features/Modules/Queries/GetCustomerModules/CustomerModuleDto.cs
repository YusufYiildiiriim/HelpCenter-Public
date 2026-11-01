namespace HelpCenter.Application.Features.Modules.Queries.GetCustomerModules;

public class CustomerModuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RequestedLocale { get; set; } = HelpCenter.Domain.Common.SupportedLocales.Default;
    public string? ContentLocale { get; set; }
}
