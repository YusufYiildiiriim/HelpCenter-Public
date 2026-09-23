using MediatR;

namespace HelpCenter.Application.Features.Customers.Queries.GetProfile;

public record GetProfileQuery(int CustomerId) : IRequest<CustomerProfileDto>;
