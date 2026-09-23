using System.Linq.Expressions;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Features.Requests.Queries.GetAdminRequests;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Features.Requests.Queries.GetAssignedRequests;

public class GetAssignedRequestsQueryHandler : IRequestHandler<GetAssignedRequestsQuery, PaginatedResponse<AdminRequestDto>>
{
    private readonly IUnitOfWork _unitofwork;
    private readonly ILogger<GetAssignedRequestsQueryHandler> _logger;
    private readonly IUserContext _userContext;

    public GetAssignedRequestsQueryHandler(
        IUnitOfWork unitofwork,
        ILogger<GetAssignedRequestsQueryHandler> logger,
        IUserContext userContext)
    {
        _unitofwork = unitofwork;
        _logger = logger;
        _userContext = userContext;
    }

    public async Task<PaginatedResponse<AdminRequestDto>> Handle(GetAssignedRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;
        _logger.LogInformation("Kullanıcıya atanan talepler listeleniyor. UserId={UserId}", userId);

        Expression<Func<CustomerRequest, bool>> predicate = x =>
            (x.AssignedUserId == userId || x.CurrentExpertId == userId) &&
            (string.IsNullOrEmpty(request.Status) || x.Status.Name == request.Status) &&
            (!request.Priority.HasValue || x.Priority == request.Priority.Value);

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

        _logger.LogInformation("Atanan talepler başarıyla listelendi. UserId={UserId}, Toplam={Count}", userId, response.TotalCount);

        return response;
    }
}
