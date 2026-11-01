using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetCustomerModules;

public class GetCustomerModulesQuery : IRequest<List<CustomerModuleDto>>
{
    public Guid CustomerId { get; set; }
    public Guid CompanyId { get; set; }
    public string Locale { get; set; } = HelpCenter.Domain.Common.SupportedLocales.Default;
}
