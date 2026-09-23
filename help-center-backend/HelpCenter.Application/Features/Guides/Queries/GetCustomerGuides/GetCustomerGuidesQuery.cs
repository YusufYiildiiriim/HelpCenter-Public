using HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Queries.GetCustomerGuides;

public record GetCustomerGuidesQuery() : IRequest<List<PublicGuideDto>>;
