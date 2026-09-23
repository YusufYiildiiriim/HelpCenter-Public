namespace HelpCenter.Application.Interfaces;

/// <summary>
/// Primitive for writing critical operations to the audit log.
/// Handlers use this interface instead of <see cref="EfContext"/> directly;
/// the implementation enriches Actor / IP / User-Agent / CorrelationId from HttpContext.
/// </summary>
public interface IAuditLogWriter
{
    Task WriteAsync(string eventType, object? metadata = null, CancellationToken ct = default);

    Task WriteAsync(
        string eventType,
        string targetType,
        int? targetId,
        object? metadata = null,
        CancellationToken ct = default);

    Task WriteDiffAsync(
        string eventType,
        string targetType,
        int targetId,
        object? before,
        object? after,
        CancellationToken ct = default);
}
