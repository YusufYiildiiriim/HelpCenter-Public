using MediatR;

namespace HelpCenter.Application.Features.Organization.Queries.GetOrganizationInfo;

public record GetOrganizationInfoQuery() : IRequest<OrganizationInfoDto?>;
