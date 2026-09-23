using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetCustomerMessages;

public class GetCustomerMessagesQuery : IRequest<List<CustomerMessageDto>>
{
    public Guid RequestPublicId { get; set; }
    public int CustomerId { get; set; }
}
