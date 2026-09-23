using HelpCenter.Application.Features.Requests.Queries.GetCustomerRequests;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetCustomerRequestById;

public class GetCustomerRequestByIdQuery : IRequest<CustomerRequestDto>
{
    public Guid PublicId { get; set; }
    public int CustomerId { get; set; }
}
