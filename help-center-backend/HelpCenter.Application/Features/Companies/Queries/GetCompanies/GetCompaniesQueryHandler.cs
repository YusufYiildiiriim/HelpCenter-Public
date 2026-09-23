using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Companies.Queries.GetCompanies;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, PaginatedResponse<GetCompaniesResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCompaniesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<GetCompaniesResponse>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim().ToLower();

        Expression<Func<Company, bool>> predicate = x =>
            string.IsNullOrWhiteSpace(search) || x.Name.ToLower().Contains(search);

        var paginatedEntities = await _unitOfWork.Repository<Company>().GetPaginatedAsync(
            predicate,
            request.PageNumber,
            request.PageSize,
            cancellationToken: cancellationToken,
            x => x.CompanyModules, x => x.Project!);

        var dtoItems = _mapper.Map<List<GetCompaniesResponse>>(paginatedEntities.Items);

        return new PaginatedResponse<GetCompaniesResponse>
        {
            Items = dtoItems,
            TotalCount = paginatedEntities.TotalCount,
            TotalPages = paginatedEntities.TotalPages,
            CurrentPage = paginatedEntities.CurrentPage,
            PageSize = paginatedEntities.PageSize
        };
    }
}
