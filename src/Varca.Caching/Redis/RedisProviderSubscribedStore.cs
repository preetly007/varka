using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.Logging;
using Unica.Core.Contracts.Models;
using Varca.Domain.Models;
using Varca.Domain.Repositories;

namespace Varca.Caching.Redis;

public class RedisProviderSubscribedStore : IProviderSubscribedStore
{
    private readonly IProviderSubscribedStore _providerSubscribedStore;
    private readonly ILogger<RedisProviderSubscribedStore> _logger;
    private const string prefix = "varca-cache-clientproviderinfo";
    private readonly CacheProvider _cacheProvider;

    public RedisProviderSubscribedStore(ILogger<RedisProviderSubscribedStore> logger, IProviderSubscribedStore providerSubscribedStore, CacheProvider cacheProvider) {
        _providerSubscribedStore = providerSubscribedStore;
        _cacheProvider = cacheProvider;
        _logger = logger;
    }
    public async Task<List<ClientProviderInfo>> GetManyAsync(string accountId, bool enabled = true)
    {
        var cacheKey = $"{prefix}-{accountId}-{enabled}".ToLower();
        var providers = await _cacheProvider.GetAsync<List<ClientProviderInfo>>(cacheKey);
        if (providers == null) {
            providers = await _providerSubscribedStore.GetManyAsync(accountId, enabled);
            if (providers.Count > 0) {
                _ = _cacheProvider.SetAsync(cacheKey, providers);
            }
        }
        return providers;
    }

    public async Task<List<ProviderPublishKey>> GetManyPublishKeyAsync(string accountId, bool enabled = true)
    {
        var cacheKey = $"{prefix}-publishkeys-{accountId}-{enabled}".ToLower();
        var publishKeys = await _cacheProvider.GetAsync<List<ProviderPublishKey>>(cacheKey);
        if (publishKeys == null) {
            publishKeys = await _providerSubscribedStore.GetManyPublishKeyAsync(accountId, enabled);
            if (publishKeys.Count > 0) {
                _ = _cacheProvider.SetAsync(cacheKey, publishKeys);
            }
        }
        return publishKeys;
    }
}
