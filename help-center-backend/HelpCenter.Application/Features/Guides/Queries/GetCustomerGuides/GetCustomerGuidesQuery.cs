using HelpCenter.Application.Features.Guides.Queries.GetPublicGuides;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Queries.GetCustomerGuides;

public record GetCustomerGuidesQuery(string Locale, Guid CompanyId) : IRequest<List<PublicGuideDto>>;
