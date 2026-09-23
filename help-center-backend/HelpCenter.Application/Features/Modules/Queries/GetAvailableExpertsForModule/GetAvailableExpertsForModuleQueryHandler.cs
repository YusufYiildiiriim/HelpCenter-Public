using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Modules.Queries.GetAvailableExpertsForModule;

public class GetAvailableExpertsForModuleQueryHandler : IRequestHandler<GetAvailableExpertsForModuleQuery, List<LookupItem>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAvailableExpertsForModuleQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<LookupItem>> Handle(GetAvailableExpertsForModuleQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<User>().SelectAsync(
            u => u.IsActive,
            u => new LookupItem(u.Id, u.Account.FirstName + " " + u.Account.LastName),
            cancellationToken: cancellationToken);
    }
}
