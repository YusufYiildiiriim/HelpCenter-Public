using AutoMapper;
using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Organization.Queries.GetOrganizationInfo;

public class GetOrganizationInfoQueryHandler : IRequestHandler<GetOrganizationInfoQuery, OrganizationInfoDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetOrganizationInfoQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public Task<OrganizationInfoDto?> Handle(GetOrganizationInfoQuery request, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            CacheKeys.OrganizationInfoAdmin(),
            CacheKeys.OrganizationInfoPrefix,
            async ct =>
            {
                var org = await _unitOfWork.Repository<OrganizationInfo>().FirstOrDefaultAsync(
                    x => x.IsActive,
                    cancellationToken: ct);

                return org == null ? null : _mapper.Map<OrganizationInfoDto>(org);
            },
            CacheKeys.PublicTtl,
            cancellationToken);
    }
}
