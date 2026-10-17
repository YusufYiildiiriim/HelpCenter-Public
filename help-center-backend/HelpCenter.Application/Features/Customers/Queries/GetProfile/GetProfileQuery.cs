using MediatR;

namespace HelpCenter.Application.Features.Customers.Queries.GetProfile;

public record GetProfileQuery(Guid CustomerId) : IRequest<CustomerProfileDto>;
