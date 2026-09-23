using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Organization.Queries.GetPublicOrganizationInfo;

public class GetPublicOrganizationInfoQueryHandler : IRequestHandler<GetPublicOrganizationInfoQuery, PublicOrganizationInfoDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetPublicOrganizationInfoQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public Task<PublicOrganizationInfoDto?> Handle(GetPublicOrganizationInfoQuery request, CancellationToken cancellationToken)
    {
        return _cache.GetOrCreateAsync(
            CacheKeys.OrganizationInfoPublic(),
            CacheKeys.OrganizationInfoPrefix,
            async ct =>
            {
                var org = await _unitOfWork.Repository<OrganizationInfo>().FirstOrDefaultAsync(
                    x => x.IsActive,
                    cancellationToken: ct);

                if (org == null) return null;

                return new PublicOrganizationInfoDto
                {
                    OrganizationName = org.OrganizationName,
                    LogoUrl = org.LogoUrl,
                    FooterText = org.FooterText
                };
            },
            CacheKeys.PublicTtl,
            cancellationToken);
    }
}
