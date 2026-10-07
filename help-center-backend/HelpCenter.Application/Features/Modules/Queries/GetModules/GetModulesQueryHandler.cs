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
    private readonly IModuleUsageReadRepository _usage;

    public GetModulesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetModulesQueryHandler> logger, IModuleUsageReadRepository usage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _usage = usage;
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

        var usage = await _usage.CountAsync(
            dtoItems.Select(module => new ModuleUsageLookup(module.Id, module.Name)).ToArray(),
            request.OnlyActive,
            cancellationToken);

        foreach (var dto in dtoItems)
        {
            dto.UsageCount = usage.GetValueOrDefault(dto.Id);
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
