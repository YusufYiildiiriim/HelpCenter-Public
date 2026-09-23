using HelpCenter.Application.Exceptions;
using HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;

namespace HelpCenter.Application.Features.Requests.Queries.GetAdminRequestById;

/// <summary>
/// Rules belonging only to the admin request detail slice.
/// </summary>
public static class GetAdminRequestByIdRules
{
    /// <summary>
    /// Verifies that the request exists in the system. A separate `IsDeleted` check is not needed —
    /// the projection query already goes through the default (filtered) query.
    /// </summary>
    public static void RequestShouldExist(AdminRequestDto? dto)
    {
        if (dto == null)
        {
            throw new RequestNotFoundException();
        }
    }
}
