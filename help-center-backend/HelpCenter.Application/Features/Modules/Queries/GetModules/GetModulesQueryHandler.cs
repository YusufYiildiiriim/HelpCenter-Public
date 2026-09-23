using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Modules.Queries.GetModules;

public class GetModulesQueryHandler : IRequestHandler<GetModulesQuery, PaginatedResponse<ModuleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetModulesQueryHandler> _logger;

    public GetModulesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetModulesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ModuleDto>> Handle(GetModulesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Modüller listeleniyor. Filtre: Sadece Aktif={OnlyActive}", request.OnlyActive);

        var search = request.Search?.Trim().ToLower();

        Expression<Func<Module, bool>> predicate = x =>
            (!request.OnlyActive || x.IsActive) &&
            (string.IsNullOrWhiteSpace(search) || x.Name.ToLower().Contains(search));

        var paginatedEntities = await _unitOfWork.Repository<Module>().GetPaginatedAsync(
            predicate,
            request.PageNumber,
            request.PageSize,
            cancellationToken: cancellationToken);

        var dtoItems = _mapper.Map<List<ModuleDto>>(paginatedEntities.Items);

        var guides = await _unitOfWork.Repository<Guide>().FindAsync(
            x => !request.OnlyActive || x.IsActive,
            cancellationToken);

        var customerRequests = await _unitOfWork.Repository<CustomerRequest>().GetAllAsync(
            cancellationToken);

        var guideList = guides.ToList();
        var requestList = customerRequests.ToList();

        foreach (var dto in dtoItems)
        {
            var guideCount = guideList.Count(r => r.Module == dto.Name);
            var requestCount = requestList.Count(r => r.ModuleId == dto.Id);
            dto.UsageCount = guideCount + requestCount;
        }

        _logger.LogInformation("{Count} adet modül başarıyla listelendi.", dtoItems.Count);

        return new PaginatedResponse<ModuleDto>
        {
            Items = dtoItems,
            TotalCount = paginatedEntities.TotalCount,
            TotalPages = paginatedEntities.TotalPages,
            CurrentPage = paginatedEntities.CurrentPage,
            PageSize = paginatedEntities.PageSize
        };
    }
}
