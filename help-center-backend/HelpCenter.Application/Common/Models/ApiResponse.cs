namespace HelpCenter.Application.Common.Models;

/// <summary>
/// Standard API response envelope. Provides a consistent shape across all endpoints.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public T? Data { get; set; }
    public ApiError? Error { get; set; }
    public string? CorrelationId { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> ErrorResult(string message, string? code = null, IReadOnlyList<string>? details = null)
        => new()
        {
            Success = false,
            Message = message,
            Error = new ApiError { Code = code ?? "Error", Message = message, Details = details }
        };
}

/// <summary>
/// Shortcut for cases that don't require generics (especially error responses).
/// </summary>
public sealed class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Fail(string message, string? code = null, IReadOnlyList<string>? details = null)
    {
        var r = new ApiResponse { Success = false, Message = message };
        r.Error = new ApiError { Code = code ?? "Error", Message = message, Details = details };
        return r;
    }

    public static ApiResponse Ok(string? message = null)
        => new() { Success = true, Message = message };
}

/// <summary>
/// Structured detail block for error responses.
/// <c>Code</c> is a machine-readable short name (e.g. <c>ValidationError</c>, <c>NotFound</c>),
/// <c>Details</c> contains field-level error messages.
/// </summary>
public sealed class ApiError
{
    public string Code { get; set; } = "Error";
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string>? Details { get; set; }
}
