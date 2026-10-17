using HelpCenter.Application.Common.Models;
using MediatR;

namespace HelpCenter.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQuery : IRequest<PaginatedResponse<AuditLogDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? EventType { get; set; }
    public Guid? ActorUserId { get; set; }
    public string? TargetType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

// Excludes email, IP, user-agent, before/after JSON and arbitrary metadata.
public record AuditLogDto(Guid Id, string EventType, Guid? ActorUserId, string? TargetType, Guid? TargetId, string? CorrelationId, DateTime OccurredAt);
