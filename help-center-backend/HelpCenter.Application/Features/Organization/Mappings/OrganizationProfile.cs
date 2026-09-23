using AutoMapper;
using HelpCenter.Application.Features.Organization.Queries.GetOrganizationInfo;
using HelpCenter.Application.Features.Organization.Queries.GetPublicOrganizationInfo;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Organization.Mappings;

public class OrganizationProfile : AutoMapper.Profile
{
    public OrganizationProfile()
    {
        CreateMap<OrganizationInfo, OrganizationInfoDto>();
        CreateMap<OrganizationInfo, PublicOrganizationInfoDto>();
    }
}
