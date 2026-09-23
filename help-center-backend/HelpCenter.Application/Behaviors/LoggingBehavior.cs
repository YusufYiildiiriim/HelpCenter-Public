using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HelpCenter.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var uniqueId = Guid.NewGuid().ToString();
        var timer = Stopwatch.StartNew();

        try
        {
            var response = await next();
            timer.Stop();

            _logger.LogInformation("[{RequestId}] Completed {RequestName} in {ElapsedMilliseconds}ms",
                uniqueId, requestName, timer.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();

            _logger.LogError(ex, "[{RequestId}] Failed {RequestName} after {ElapsedMilliseconds}ms",
                uniqueId, requestName, timer.ElapsedMilliseconds);
            throw;
        }
    }
}
