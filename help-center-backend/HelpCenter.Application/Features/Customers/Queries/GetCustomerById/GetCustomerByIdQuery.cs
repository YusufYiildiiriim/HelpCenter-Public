using HelpCenter.Application.Features.Customers.Queries.GetCustomers;
using MediatR;

namespace HelpCenter.Application.Features.Customers.Queries.GetCustomerById;

public record GetCustomerByIdQuery(Guid PublicId) : IRequest<CustomerDetailDto>;
