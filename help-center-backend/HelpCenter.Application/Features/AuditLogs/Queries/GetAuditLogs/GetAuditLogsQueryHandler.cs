using System.Linq.Expressions;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using MediatR;

namespace HelpCenter.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAuditLogsQuery, PaginatedResponse<AuditLogDto>>
{
    public Task<PaginatedResponse<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        Expression<Func<AuditLog, bool>> filter = log =>
            (string.IsNullOrEmpty(request.EventType) || log.EventType == request.EventType) &&
            (!request.ActorUserId.HasValue || log.ActorUserId == request.ActorUserId) &&
            (string.IsNullOrEmpty(request.TargetType) || log.TargetType == request.TargetType) &&
            (!request.From.HasValue || log.OccurredAt >= request.From) &&
            (!request.To.HasValue || log.OccurredAt <= request.To);
        return unitOfWork.Repository<AuditLog>().GetPaginatedProjectedAsync(filter,
            log => new AuditLogDto(log.Id, log.EventType, log.ActorUserId, log.TargetType, log.TargetId, log.CorrelationId, log.OccurredAt),
            request.PageNumber, request.PageSize, orderByDescendingKey: log => log.Id, cancellationToken: cancellationToken);
    }
}
