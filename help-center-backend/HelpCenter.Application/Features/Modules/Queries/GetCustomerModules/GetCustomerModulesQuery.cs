using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetCustomerModules;

public class GetCustomerModulesQuery : IRequest<List<CustomerModuleDto>>
{
    public int CustomerId { get; set; }
    public int CompanyId { get; set; }
}
