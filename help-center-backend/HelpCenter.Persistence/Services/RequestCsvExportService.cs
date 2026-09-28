using System.Runtime.CompilerServices;
using HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.Persistence.Services;

public sealed class RequestCsvExportService : IRequestCsvExportService
{
    private readonly EfContext _db;

    public RequestCsvExportService(EfContext db) => _db = db;

    public async IAsyncEnumerable<RequestExportRowDto> StreamAsync(
        GetAdminRequestsQuery filter,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var search = filter.Search?.Trim().ToLower();
        IQueryable<CustomerRequest> query = _db.CustomerRequests.AsNoTracking().Where(x =>
            (!filter.CompanyPublicId.HasValue || x.Customer.Company.PublicId == filter.CompanyPublicId.Value) &&
            (!filter.CustomerId.HasValue || x.CustomerId == filter.CustomerId.Value) &&
            (string.IsNullOrEmpty(filter.Status) || x.Status.Name == filter.Status) &&
            (!filter.Priority.HasValue || x.Priority == filter.Priority.Value) &&
            (!filter.AssignedUserId.HasValue || x.AssignedUserId == filter.AssignedUserId.Value) &&
            (string.IsNullOrWhiteSpace(search) || x.Title.ToLower().Contains(search) || x.Description.ToLower().Contains(search)));

        var rows = query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(x => new RequestExportRowDto
            {
                TicketId = x.TicketId,
                Title = x.Title,
                Description = x.Description,
                CompanyName = x.Customer.Company.Name,
                CustomerName = x.Customer.Account.FirstName + " " + x.Customer.Account.LastName,
                ModuleName = x.Module != null ? x.Module.Name : string.Empty,
                PriorityName = x.Priority.ToString(),
                Status = x.Status.Name,
                AssignedUserName = x.AssignedUser != null ? x.AssignedUser.Account.FirstName + " " + x.AssignedUser.Account.LastName : "Atanmadı",
                MessageCount = x.Conversation != null ? x.Conversation.Messages.Count : 0,
                CreatedAt = x.CreatedAt
            })
            .AsAsyncEnumerable();

        await foreach (var row in rows.WithCancellation(cancellationToken))
            yield return row;
    }
}
