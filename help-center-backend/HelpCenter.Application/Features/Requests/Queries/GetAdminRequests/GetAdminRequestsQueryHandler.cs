using System.Linq.Expressions;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;

public class GetAdminRequestsQueryHandler : IRequestHandler<GetAdminRequestsQuery, PaginatedResponse<AdminRequestDto>>
{
    private readonly IUnitOfWork _unitofwork;
    private readonly ILogger<GetAdminRequestsQueryHandler> _logger;

    public GetAdminRequestsQueryHandler(IUnitOfWork unitofwork, ILogger<GetAdminRequestsQueryHandler> logger)
    {
        _unitofwork = unitofwork;
        _logger = logger;
    }

    public async Task<PaginatedResponse<AdminRequestDto>> Handle(GetAdminRequestsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin talepleri listeleniyor. CompanyPublicId={CompanyPublicId}, CustomerId={CustomerId}, Status={Status}",
            request.CompanyPublicId, request.CustomerId, request.Status);

        var search = request.Search?.Trim().ToLower();

        Expression<Func<CustomerRequest, bool>> predicate = x =>
            (!request.CustomerId.HasValue || x.CustomerId == request.CustomerId.Value) &&
            (!request.CompanyPublicId.HasValue || x.Customer.Company.PublicId == request.CompanyPublicId.Value) &&
            (string.IsNullOrEmpty(request.Status) || x.Status.Name == request.Status) &&
            (!request.Priority.HasValue || x.Priority == request.Priority.Value) &&
            (!request.AssignedUserId.HasValue || x.AssignedUserId == request.AssignedUserId.Value) &&
            (string.IsNullOrWhiteSpace(search) || x.Title.ToLower().Contains(search) || x.Description.ToLower().Contains(search));

        Expression<Func<CustomerRequest, AdminRequestDto>> selector = x => new AdminRequestDto
        {
            Id = x.Id,
            PublicId = x.PublicId,
            TicketId = x.TicketId,
            Title = x.Title,
            Description = x.Description,
            StatusId = x.StatusId,
            Status = x.Status.Name,
            Priority = x.Priority,
            PriorityName = x.Priority.ToString(),
            ModuleId = x.ModuleId,
            ModuleName = x.Module != null ? x.Module.Name : string.Empty,
            CustomerName = x.Customer.Account.FirstName + " " + x.Customer.Account.LastName,
            CompanyName = x.Customer.Company.Name,
            AssignedUserName = x.AssignedUser != null ? x.AssignedUser.Account.FirstName + " " + x.AssignedUser.Account.LastName : "Atanmadı",
            CurrentExpertName = x.CurrentExpert != null ? x.CurrentExpert.Account.FirstName + " " + x.CurrentExpert.Account.LastName : string.Empty,
            RequestSubjectName = x.RequestSubject != null ? x.RequestSubject.Name : string.Empty,
            DocumentCount = x.Documents.Count,
            MessageCount = x.Conversation != null ? x.Conversation.Messages.Count : 0,
            CreatedAt = x.CreatedAt
        };

        var response = await _unitofwork.Repository<CustomerRequest>().GetPaginatedProjectedAsync(
            predicate,
            selector,
            request.PageNumber,
            request.PageSize,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Admin talepleri başarıyla listelendi. Toplam: {Count}, Sayfa: {Page}", response.TotalCount, request.PageNumber);

        return response;
    }
}
