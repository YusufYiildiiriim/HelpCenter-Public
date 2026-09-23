using System.Security.Claims;
using System.Text.Json;
using HelpCenter.Application.Interfaces;
using HelpCenter.Domain.Entities;
using HelpCenter.Persistence.Context;
using Microsoft.AspNetCore.Http;

namespace HelpCenter.Persistence.Services;

/// <summary>
/// Implementation of <see cref="IAuditLogWriter"/>. Enriches Actor / IP /
/// User-Agent / TraceIdentifier from the HttpContext and stages the audit row in the
/// request-scoped <see cref="EfContext"/>. When a handler calls this before its unit of work
/// save, the business change and audit row are committed by the same EF Core transaction.
/// </summary>
public class AuditLogWriter : IAuditLogWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private readonly EfContext _db;
    private readonly IHttpContextAccessor _httpAccessor;

    public AuditLogWriter(EfContext db, IHttpContextAccessor httpAccessor)
    {
        _db = db;
        _httpAccessor = httpAccessor;
    }

    public Task WriteAsync(string eventType, object? metadata = null, CancellationToken ct = default)
        => WriteInternalAsync(eventType, targetType: null, targetId: null, before: null, after: null, metadata, ct);

    public Task WriteAsync(string eventType, string targetType, int? targetId, object? metadata = null, CancellationToken ct = default)
        => WriteInternalAsync(eventType, targetType, targetId, before: null, after: null, metadata, ct);

    public Task WriteDiffAsync(string eventType, string targetType, int targetId, object? before, object? after, CancellationToken ct = default)
        => WriteInternalAsync(eventType, targetType, targetId, before, after, metadata: null, ct);

    private async Task WriteInternalAsync(
        string eventType,
        string? targetType,
        int? targetId,
        object? before,
        object? after,
        object? metadata,
        CancellationToken ct)
    {
        var http = _httpAccessor.HttpContext;
        var user = http?.User;

        var actorId = TryParseInt(user?.FindFirst("UserId")?.Value);
        var actorEmail = user?.FindFirst(ClaimTypes.Email)?.Value
                         ?? user?.FindFirst("Email")?.Value;

        var entry = new AuditLog
        {
            EventType = eventType,
            ActorUserId = actorId,
            ActorEmail = actorEmail,
            IpAddress = http?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = http?.Request.Headers.UserAgent.ToString(),
            CorrelationId = http?.TraceIdentifier,
            TargetType = targetType,
            TargetId = targetId,
            BeforeJson = before == null ? null : JsonSerializer.Serialize(before, JsonOptions),
            AfterJson = after == null ? null : JsonSerializer.Serialize(after, JsonOptions),
            MetadataJson = metadata == null ? null : JsonSerializer.Serialize(metadata, JsonOptions),
            OccurredAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(entry);
        await _db.SaveChangesAsync(ct);
    }

    private static int? TryParseInt(string? value)
        => int.TryParse(value, out var n) ? n : null;
}
