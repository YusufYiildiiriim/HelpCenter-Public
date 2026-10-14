using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;

namespace HelpCenter.Application.Common.Lookups;

/// <summary>Shared active-user pool for project membership and module expert pickers.</summary>
public sealed class UserLookupReader(IUnitOfWork unitOfWork)
{
    public Task<List<LookupItem>> ReadActiveAsync(CancellationToken cancellationToken) =>
        unitOfWork.Repository<User>().SelectAsync(
            user => user.IsActive,
            user => new LookupItem(user.Id, user.Account.FirstName + " " + user.Account.LastName),
            cancellationToken: cancellationToken);
}
