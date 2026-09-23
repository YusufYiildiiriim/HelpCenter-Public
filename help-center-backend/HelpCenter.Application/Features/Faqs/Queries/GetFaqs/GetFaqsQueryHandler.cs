using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Faqs.Queries.GetFaqs;

public class GetFaqsQueryHandler : IRequestHandler<GetFaqsQuery, PaginatedResponse<FaqDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetFaqsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<FaqDto>> Handle(GetFaqsQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim().ToLower();

        Expression<Func<FAQ, bool>> predicate = x =>
            (string.IsNullOrWhiteSpace(search) || x.Title.ToLower().Contains(search) || x.Description.ToLower().Contains(search))
            && (!request.ProjectId.HasValue || x.ProjectId == request.ProjectId.Value)
            && (!request.ModuleId.HasValue || x.ModuleId == request.ModuleId.Value);

        var paginatedEntities = await _unitOfWork.Repository<FAQ>().GetPaginatedAsync(
            predicate,
            request.PageNumber,
            request.PageSize,
            cancellationToken: cancellationToken,
            x => x.Project!, x => x.Module!);

        var dtoItems = _mapper.Map<List<FaqDto>>(paginatedEntities.Items);

        return new PaginatedResponse<FaqDto>
        {
            Items = dtoItems,
            TotalCount = paginatedEntities.TotalCount,
            TotalPages = paginatedEntities.TotalPages,
            CurrentPage = paginatedEntities.CurrentPage,
            PageSize = paginatedEntities.PageSize
        };
    }
}
