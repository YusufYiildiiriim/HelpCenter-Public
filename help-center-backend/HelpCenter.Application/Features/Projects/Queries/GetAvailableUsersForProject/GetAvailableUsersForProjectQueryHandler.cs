using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.Projects.Queries.GetAvailableUsersForProject;

public class GetAvailableUsersForProjectQueryHandler : IRequestHandler<GetAvailableUsersForProjectQuery, List<LookupItem>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAvailableUsersForProjectQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<LookupItem>> Handle(GetAvailableUsersForProjectQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<User>().SelectAsync(
            u => u.IsActive,
            u => new LookupItem(u.Id, u.Account.FirstName + " " + u.Account.LastName),
            cancellationToken: cancellationToken);
    }
}
