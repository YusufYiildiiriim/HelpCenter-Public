using HelpCenter.Application.Common.Caching;
using HelpCenter.Application.Interfaces;
using MediatR;

namespace HelpCenter.Application.Behaviors;

public class MessageCacheInvalidationBehavior<TRequest, TResponse>(ICacheService cache) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is not IInvalidatesMessageCache) return await next();
        try { return await next(); }
        finally { cache.Invalidate(CacheCategory.Messages); }
    }
}
