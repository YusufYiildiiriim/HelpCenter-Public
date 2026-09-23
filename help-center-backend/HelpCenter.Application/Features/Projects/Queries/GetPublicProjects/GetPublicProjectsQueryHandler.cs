using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetPublicProjects;

public class GetPublicProjectsQueryHandler : IRequestHandler<GetPublicProjectsQuery, List<PublicProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetPublicProjectsQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public Task<List<PublicProjectDto>> Handle(GetPublicProjectsQuery request, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            CacheKeys.PublicProjects(),
            CacheKeys.PublicProjectsPrefix,
            async ct =>
            {
                var projects = await _unitOfWork.Repository<Project>()
                    .FindAsync(x => x.IsActive, ct);

                return projects.Select(p => new PublicProjectDto
                {
                    Id = p.Id,
                    PublicId = p.PublicId,
                    Name = p.Name,
                    Description = p.Description
                }).ToList();
            },
            CacheKeys.PublicTtl,
            cancellationToken);
    }
}
