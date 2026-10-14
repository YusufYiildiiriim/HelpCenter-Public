using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Common.Lookups;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetAvailableExpertsForModule;

public class GetAvailableExpertsForModuleQueryHandler : IRequestHandler<GetAvailableExpertsForModuleQuery, List<LookupItem>>
{
    private readonly UserLookupReader _reader;

    public GetAvailableExpertsForModuleQueryHandler(UserLookupReader reader)
    {
        _reader = reader;
    }

    public Task<List<LookupItem>> Handle(GetAvailableExpertsForModuleQuery request, CancellationToken cancellationToken) =>
        _reader.ReadActiveAsync(cancellationToken);
}
