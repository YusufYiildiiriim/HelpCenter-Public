using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetCustomerRequests;

public class GetCustomerRequestsQuery : IRequest<List<CustomerRequestDto>>
{
    public Guid CustomerId { get; set; }
    public string? Status { get; set; }
}
