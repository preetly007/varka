
using Amazon.Runtime.Internal.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Varca.Caching.Redis;
using Varca.Domain.Models;
using Varca.Domain.Repositories;

namespace Varca.Domain.Services;

public class AccountService : IAccountService
{
    private readonly IProviderSubscribedStore _providerSubscribedStore;
    private readonly ILogger<AccountService> _logger;
    public AccountService(ILogger<AccountService> logger,
             [FromKeyedServices(CacheProvider.Cacheable)] IProviderSubscribedStore providerSubscribedStore) {
        _providerSubscribedStore = providerSubscribedStore;
        _logger = logger;
    }

    private async Task<List<ProviderPublishKey>> GetProviderPublishKeysAsync(string accountId) 
    {
        var configuredEnabledPublishKeys = await _providerSubscribedStore.GetManyPublishKeyAsync(accountId, enabled: true).ConfigureAwait(false);
        return configuredEnabledPublishKeys;
    }

    
    public async Task<SearchMappingsGeometry> BuildAccessKeysFilterAsync(SearchMappingsGeometry searchMappingsGeometry)
    {
        searchMappingsGeometry.Filters ??= new Filter();
        searchMappingsGeometry.Filters.Providers ??= [];
        searchMappingsGeometry.Filters.AccessKeys ??= [];

        var filters = searchMappingsGeometry.Filters;
        var configuredEnabledPublishKeys = await GetProviderPublishKeysAsync(searchMappingsGeometry.AccountId).ConfigureAwait(false);
        FillAccessKeysFilter(configuredEnabledPublishKeys, filters);
        return searchMappingsGeometry;
    }

    public async Task<PitGeometryNew> BuildAccessKeysFilterAsync(PitGeometryNew pitNew)
    {
        pitNew.Filters ??= new Filter();
        pitNew.Filters.Providers ??= [];
        pitNew.Filters.AccessKeys ??= [];

        var configuredEnabledPublishKeys = await GetProviderPublishKeysAsync(pitNew.AccountId).ConfigureAwait(false);
        var filters = pitNew.Filters;
        FillAccessKeysFilter(configuredEnabledPublishKeys, filters);
        return pitNew;
    }

    public async Task<PitPlaceNew> BuildAccessKeysFilterAsync(PitPlaceNew pitNew)
    {
        pitNew.Filters ??= new Filter();
        pitNew.Filters.Providers ??= [];
        pitNew.Filters.AccessKeys ??= [];

        var configuredEnabledPublishKeys = await GetProviderPublishKeysAsync(pitNew.AccountId).ConfigureAwait(false);
        var filters = pitNew.Filters;
        FillAccessKeysFilter(configuredEnabledPublishKeys, filters);
        return pitNew;
    }

    private static void FillAccessKeysFilter(List<ProviderPublishKey> configuredEnabledPublishKeys, Filter filters) {
        configuredEnabledPublishKeys.ForEach(x => {
            var filterProvider = filters.Providers.FirstOrDefault(a => x.ProviderFamily.Equals(a.Name, StringComparison.InvariantCultureIgnoreCase));
            
            if (filters.Providers.Count == 0 || filterProvider != null) {
                if (x.AvailableChannelIds == null || x.AvailableChannelIds.Count == 0) {
                    if (!filters.AccessKeys.Contains(x.PublishKey)) {
                        filters.AccessKeys.Add(x.PublishKey);
                    }
                }
                else if (x.ApprovedChannelIds != null && x.ApprovedChannelIds.Count > 0) {
                    foreach (var approvedChannelId in x.ApprovedChannelIds) {
                        if (filterProvider != null && filterProvider.ChannelIds != null && filterProvider.ChannelIds.Count > 0) {
                            var filterChannelId = filterProvider.ChannelIds.FirstOrDefault(a => a.Equals(approvedChannelId, StringComparison.InvariantCultureIgnoreCase));
                            if (!string.IsNullOrEmpty(filterChannelId)) {
                                var accessKey = GeoHotelMapping.FormatAccessKey(x.PublishKey, approvedChannelId);
                                if (!filters.AccessKeys.Contains(accessKey)) {
                                    filters.AccessKeys.Add(accessKey);
                                }
                            }
                        }
                        else {
                            var accessKey = GeoHotelMapping.FormatAccessKey(x.PublishKey, approvedChannelId);
                            if (!filters.AccessKeys.Contains(accessKey)) {
                                filters.AccessKeys.Add(accessKey);
                            }
                        }
                    }
                }
            }
        });
    }

    public async Task<SearchMappingsPlace> BuildAccessKeysFilterAsync(SearchMappingsPlace searchPlace)
    {
        searchPlace.Filters ??= new Filter();
        searchPlace.Filters.Providers ??= [];
        searchPlace.Filters.AccessKeys ??= [];

        var configuredEnabledPublishKeys = await GetProviderPublishKeysAsync(searchPlace.AccountId).ConfigureAwait(false);
        var filters = searchPlace.Filters;
        FillAccessKeysFilter(configuredEnabledPublishKeys, filters);
        return searchPlace;
    }
}

