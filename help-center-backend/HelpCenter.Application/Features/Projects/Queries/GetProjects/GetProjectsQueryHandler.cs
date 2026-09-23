using System.Linq.Expressions;
using AutoMapper;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Projects.Queries.GetProjects;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PaginatedResponse<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetProjectsQueryHandler> _logger;

    public GetProjectsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetProjectsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Projeler listeleniyor. Filtre: Sadece Aktif={OnlyActive}", request.OnlyActive);

        var search = request.Search?.Trim().ToLower();

        Expression<Func<Project, bool>> predicate = x =>
            (!request.OnlyActive || x.IsActive) &&
            (string.IsNullOrWhiteSpace(search) || x.Name.ToLower().Contains(search));

        var paginatedEntities = await _unitOfWork.Repository<Project>().GetPaginatedAsync(
            predicate,
            request.PageNumber,
            request.PageSize,
            cancellationToken: cancellationToken,
            x => x.Companies, x => x.UserProjects, x => x.ProjectModules);

        var dtoItems = _mapper.Map<List<ProjectDto>>(paginatedEntities.Items);

        _logger.LogInformation("{Count} adet proje başarıyla listelendi.", dtoItems.Count);

        return new PaginatedResponse<ProjectDto>
        {
            Items = dtoItems,
            TotalCount = paginatedEntities.TotalCount,
            TotalPages = paginatedEntities.TotalPages,
            CurrentPage = paginatedEntities.CurrentPage,
            PageSize = paginatedEntities.PageSize
        };
    }
}
