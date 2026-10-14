using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Common.Lookups;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetAvailableUsersForProject;

public class GetAvailableUsersForProjectQueryHandler : IRequestHandler<GetAvailableUsersForProjectQuery, List<LookupItem>>
{
    private readonly UserLookupReader _reader;

    public GetAvailableUsersForProjectQueryHandler(UserLookupReader reader)
    {
        _reader = reader;
    }

    public Task<List<LookupItem>> Handle(GetAvailableUsersForProjectQuery request, CancellationToken cancellationToken) =>
        _reader.ReadActiveAsync(cancellationToken);
}
