using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQuery : IRequest<PaginatedResponse<CustomerDetailDto>>
{
    public Guid? CompanyPublicId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}
