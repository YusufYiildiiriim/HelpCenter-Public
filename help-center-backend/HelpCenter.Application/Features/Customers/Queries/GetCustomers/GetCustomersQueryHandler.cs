using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PaginatedResponse<CustomerDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCustomersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<CustomerDetailDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim().ToLower();

        Expression<Func<Customer, bool>> predicate = x =>
            (!request.CompanyPublicId.HasValue || x.Company.PublicId == request.CompanyPublicId.Value) &&
            (string.IsNullOrWhiteSpace(search) ||
             x.Account.FirstName.ToLower().Contains(search) ||
             x.Account.LastName.ToLower().Contains(search) ||
             x.Account.Email.ToLower().Contains(search));

        var paginatedEntities = await _unitOfWork.Repository<Customer>().GetPaginatedAsync(
            predicate,
            request.PageNumber,
            request.PageSize,
            cancellationToken: cancellationToken,
            x => x.Company, x => x.Account);

        var dtoItems = _mapper.Map<List<CustomerDetailDto>>(paginatedEntities.Items);

        return new PaginatedResponse<CustomerDetailDto>
        {
            Items = dtoItems,
            TotalCount = paginatedEntities.TotalCount,
            TotalPages = paginatedEntities.TotalPages,
            CurrentPage = paginatedEntities.CurrentPage,
            PageSize = paginatedEntities.PageSize
        };
    }
}
