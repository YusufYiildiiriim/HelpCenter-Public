using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetAvailableModulesForProject;

public class GetAvailableModulesForProjectQueryHandler : IRequestHandler<GetAvailableModulesForProjectQuery, List<LookupItem>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAvailableModulesForProjectQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<LookupItem>> Handle(GetAvailableModulesForProjectQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Module>().SelectAsync(
            m => m.IsActive,
            m => new LookupItem(m.Id, m.Name),
            cancellationToken: cancellationToken);
    }
}
