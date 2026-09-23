using HelpCenter.Application.Exceptions;

namespace HelpCenter.Application.Features.Statistics.Queries.GetAdminStatistics;

/// <summary>
/// Rules belonging only to the admin statistics slice.
/// </summary>
public static class GetAdminStatisticsRules
{
    /// <summary>
    /// Verifies that the user has permission to read dashboard data.
    /// </summary>
    public static void DashboardReadShouldBeAllowed(bool canRead)
    {
        if (!canRead)
        {
            throw new DashboardAccessForbiddenException();
        }
    }
}
