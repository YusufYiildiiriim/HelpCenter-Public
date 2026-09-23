using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Statuses.Queries.GetAllStatuses;

public class GetAllStatusesQueryHandler : IRequestHandler<GetAllStatusesQuery, PaginatedResponse<StatusDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllStatusesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<StatusDto>> Handle(GetAllStatusesQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim().ToLower();

        Expression<Func<CustomerRequestStatus, bool>> predicate = x =>
            string.IsNullOrWhiteSpace(search) || x.Name.ToLower().Contains(search);

        var paginatedEntities = await _unitOfWork.Repository<CustomerRequestStatus>().GetPaginatedAsync(
            predicate,
            request.PageNumber,
            request.PageSize,
            cancellationToken: cancellationToken);

        var dtoItems = _mapper.Map<List<StatusDto>>(paginatedEntities.Items);

        return new PaginatedResponse<StatusDto>
        {
            Items = dtoItems,
            TotalCount = paginatedEntities.TotalCount,
            TotalPages = paginatedEntities.TotalPages,
            CurrentPage = paginatedEntities.CurrentPage,
            PageSize = paginatedEntities.PageSize
        };
    }
}
