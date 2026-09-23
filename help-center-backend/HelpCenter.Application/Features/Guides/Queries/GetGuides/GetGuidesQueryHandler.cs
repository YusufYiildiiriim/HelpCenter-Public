using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Guides.Queries.GetGuides;

public class GetGuidesQueryHandler : IRequestHandler<GetGuidesQuery, PaginatedResponse<GuideDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetGuidesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<GuideDto>> Handle(GetGuidesQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim().ToLower();

        Expression<Func<Guide, bool>> predicate = g =>
            (string.IsNullOrWhiteSpace(search) || g.Title.ToLower().Contains(search) || g.Description.ToLower().Contains(search)) &&
            (string.IsNullOrWhiteSpace(request.Module) || request.Module == "All" || g.Module == request.Module);

        var paginatedEntities = await _unitOfWork.Repository<Guide>().GetPaginatedAsync(
            predicate,
            request.PageNumber,
            request.PageSize,
            cancellationToken: cancellationToken,
            g => g.Documents!);

        var dtoItems = _mapper.Map<List<GuideDto>>(paginatedEntities.Items);

        return new PaginatedResponse<GuideDto>
        {
            Items = dtoItems,
            TotalCount = paginatedEntities.TotalCount,
            TotalPages = paginatedEntities.TotalPages,
            CurrentPage = paginatedEntities.CurrentPage,
            PageSize = paginatedEntities.PageSize
        };
    }
}
