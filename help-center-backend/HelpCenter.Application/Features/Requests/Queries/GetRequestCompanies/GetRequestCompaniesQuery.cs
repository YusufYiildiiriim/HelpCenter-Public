using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetRequestCompanies;

public record GetRequestCompaniesQuery() : IRequest<List<RequestCompanyDto>>;
