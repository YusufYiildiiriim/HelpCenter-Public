using HelpCenter.Application.Exceptions;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Features.Organization.Commands.UpdateOrganizationInfo;

/// <summary>
/// Rules belonging only to the update-organization-info slice.
/// </summary>
public static class UpdateOrganizationInfoRules
{
    /// <summary>
    /// Verifies that the organization info exists.
    /// </summary>
    public static void OrganizationInfoShouldExist(OrganizationInfo? orgInfo)
    {
        if (orgInfo == null || orgInfo.IsDeleted)
        {
            throw new OrganizationInfoNotFoundException();
        }
    }
}
