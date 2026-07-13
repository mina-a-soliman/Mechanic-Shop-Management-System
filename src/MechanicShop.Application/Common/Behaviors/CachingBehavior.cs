using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results.Abstractions;

using MediatR;

using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly HybridCache _cache = cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (request is not ICachedQuery cachedRequest)
        {
            return await next(ct); // Go to Request Handler
        }

        _logger.LogInformation("Checking cache for {RequestName}", typeof(TRequest).Name);

       // We want to cache success responses only (not failures or validations) 
       // => So try to get from cache  
        var result = await _cache.GetOrCreateAsync<TResponse>(
            cachedRequest.CacheKey,
            _ => new ValueTask<TResponse>((TResponse)(object)null!), // factory function (in normal fetch data from db ) but here return null 
            new HybridCacheEntryOptions
            {
              // forces the system to fetch the requested value strictly from the cache, preventing it from executing the fallback factory function (prevent fetching data from DB)
                Flags = HybridCacheEntryFlags.DisableUnderlyingData 
            },
            cancellationToken: ct);


        if (result is null) 
        {
            result = await next(ct); // if result is null  => then call the Request Handler 

            if (result is IResult res && res.IsSuccess) // if the Request Handler retrieved the response from DB successfully
            {
                _logger.LogInformation("Caching result for {RequestName}", typeof(TRequest).Name);

                await _cache.SetAsync( // => Then save the result in cache
                    cachedRequest.CacheKey,
                    result,
                    new HybridCacheEntryOptions
                    {
                        Expiration = cachedRequest.Expiration
                    },
                    cachedRequest.Tags,
                    ct);
            }
        }

        return result;
    }
}