namespace HelpCenter.Domain.Entities;

/// <summary>
/// Immutable audit trail — a forensic-quality record of critical operations
/// (login, permission changes, role/user deletion, etc.). UPDATE/DELETE are
/// blocked at the DB level with a trigger; only INSERT is allowed.
/// </summary>
public sealed class AuditLog
{
    public long Id { get; set; }

    /// <summary>Machine-readable event type (e.g. "RoleCreated", "UserPasswordChanged").</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>The user who performed the operation (null for system).</summary>
    public int? ActorUserId { get; set; }
    public string? ActorEmail { get; set; }

    /// <summary>Source IP + user-agent — for forensics.</summary>
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    /// <summary>Request-level trace/correlation id — for correlating with logs.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Target entity type (e.g. "Role") and id.</summary>
    public string? TargetType { get; set; }
    public int? TargetId { get; set; }

    /// <summary>Diff of the change — optional for large payloads.</summary>
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }

    /// <summary>Event-specific free-form metadata.</summary>
    public string? MetadataJson { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
