using HelpCenter.Application.Exceptions;

namespace HelpCenter.Application.Features.Statistics.Queries.GetAdminReports;

/// <summary>
/// Rules belonging only to the admin reports slice.
/// </summary>
public static class GetAdminReportsRules
{
    /// <summary>
    /// Verifies that the user has permission to read report data.
    /// </summary>
    public static void ReportsReadShouldBeAllowed(bool canRead)
    {
        if (!canRead)
        {
            throw new ReportsAccessForbiddenException();
        }
    }
}
