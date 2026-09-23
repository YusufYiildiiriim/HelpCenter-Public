using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.RequestSubjects.Queries.GetRequestSubjects;

public class GetRequestSubjectsQueryHandler : IRequestHandler<GetRequestSubjectsQuery, PaginatedResponse<RequestSubjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRequestSubjectsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedResponse<RequestSubjectDto>> Handle(GetRequestSubjectsQuery request, CancellationToken cancellationToken)
    {
        var search = request.Search?.Trim().ToLower();

        Expression<Func<RequestSubject, bool>> predicate = x =>
            (!request.OnlyActive || x.IsActive) &&
            (string.IsNullOrWhiteSpace(search) || x.Name.ToLower().Contains(search));

        var paginatedEntities = await _unitOfWork.Repository<RequestSubject>().GetPaginatedAsync(
            predicate,
            request.PageNumber,
            request.PageSize,
            cancellationToken: cancellationToken);

        var dtoItems = _mapper.Map<List<RequestSubjectDto>>(paginatedEntities.Items);
        var paginatedIds = dtoItems.Select(x => x.Id).ToList();

        var requestSubjectIds = await _unitOfWork.Repository<CustomerRequest>().SelectAsync(
            x => x.RequestSubjectId.HasValue && paginatedIds.Contains(x.RequestSubjectId.Value),
            x => x.RequestSubjectId!.Value,
            cancellationToken: cancellationToken);

        var usageCounts = requestSubjectIds.GroupBy(id => id).ToDictionary(g => g.Key, g => g.Count());

        foreach (var dto in dtoItems)
        {
            dto.UsageCount = usageCounts.TryGetValue(dto.Id, out var count) ? count : 0;
        }

        return new PaginatedResponse<RequestSubjectDto>
        {
            Items = dtoItems,
            TotalCount = paginatedEntities.TotalCount,
            TotalPages = paginatedEntities.TotalPages,
            CurrentPage = paginatedEntities.CurrentPage,
            PageSize = paginatedEntities.PageSize
        };
    }
}
