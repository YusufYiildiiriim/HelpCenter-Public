using MediatR;

namespace HelpCenter.Application.Features.Customers.Commands.DeleteCustomer;

public record DeleteCustomerCommand(Guid PublicId) : IRequest<bool>;
