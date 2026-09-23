using MediatR;

namespace HelpCenter.Application.Features.Organization.Queries.GetPublicOrganizationInfo;

public record GetPublicOrganizationInfoQuery() : IRequest<PublicOrganizationInfoDto?>;
