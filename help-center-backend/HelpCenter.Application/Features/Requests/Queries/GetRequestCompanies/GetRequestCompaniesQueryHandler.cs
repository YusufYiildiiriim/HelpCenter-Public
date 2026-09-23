using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Requests.Queries.GetRequestCompanies;

public class GetRequestCompaniesQueryHandler : IRequestHandler<GetRequestCompaniesQuery, List<RequestCompanyDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRequestCompaniesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<RequestCompanyDto>> Handle(GetRequestCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _unitOfWork.Repository<CustomerRequest>().SelectAsync(
            x => true,
            x => new { x.Customer.Company.Id, x.Customer.Company.PublicId, x.Customer.Company.Name },
            cancellationToken: cancellationToken,
            distinct: true);

        return companies
            .DistinctBy(x => x.Id)
            .OrderBy(x => x.Name)
            .Select(x => new RequestCompanyDto
            {
                PublicId = x.PublicId,
                Name = x.Name
            })
            .ToList();
    }
}
