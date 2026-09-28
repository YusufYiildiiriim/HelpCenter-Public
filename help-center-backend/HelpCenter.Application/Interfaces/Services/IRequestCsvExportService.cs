using HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;

namespace HelpCenter.Application.Interfaces;

/// <summary>Streams filtered request rows for a download without materializing the result set.</summary>
public interface IRequestCsvExportService
{
    IAsyncEnumerable<RequestExportRowDto> StreamAsync(GetAdminRequestsQuery filter, CancellationToken cancellationToken = default);
}
