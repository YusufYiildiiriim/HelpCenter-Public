using System.Net;
using HelpCenter.Application.Common.Models;
using HelpCenter.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace HelpCenter.WebApi.Errors;

/// <summary>
/// .NET 8 <see cref="IExceptionHandler"/> implementation — replaces the traditional exception
/// middleware. Catches all exceptions in one place and wraps them in the <see cref="ApiResponse{T}"/>
/// envelope with safe messages. The stack trace never leaks to the client in production;
/// it is written to the log.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, code, safeMessage) = MapException(exception);

        var logLevel = status >= 500 ? LogLevel.Error : LogLevel.Warning;
        _logger.Log(logLevel, exception,
            "Handled exception {ExceptionType} on {Method} {Path} → {StatusCode}",
            exception.GetType().Name, httpContext.Request.Method, httpContext.Request.Path, status);

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/json";

        var details = exception switch
        {
            ValidationException v when v.Errors is { Count: > 0 } => v.Errors,
            _ => null
        };

        // Domain rule exception messages are safe and passed directly to the client.
        // For unknown server errors, a generic error message is returned in production.
        var messageToClient = exception is BaseException ? exception.Message : (_env.IsDevelopment() ? exception.Message : safeMessage);

        var payload = ApiResponse.Fail(messageToClient, code, details);
        payload.CorrelationId = httpContext.TraceIdentifier;

        await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
        return true;
    }

    private static (int status, string code, string safeMessage) MapException(Exception ex) => ex switch
    {
        BaseException baseEx           => (baseEx.StatusCode,                      baseEx.Code,         baseEx.Message),
        DbUpdateConcurrencyException   => ((int)HttpStatusCode.Conflict,          "ConcurrencyConflict", "Kayıt başka biri tarafından güncellendi. Lütfen sayfayı yenileyip tekrar deneyin."),
        OperationCanceledException     => (499,                                    "ClientClosedRequest", "İstek iptal edildi."),
        _                              => ((int)HttpStatusCode.InternalServerError, "InternalError",   "Sunucu hatası. Lütfen daha sonra tekrar deneyin.")
    };
}
