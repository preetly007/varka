using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Varca.Domain;

namespace Varca.Caching.Redis;

public class CacheProvider
{
    public const string Cacheable = "cacheable";
    private readonly IDistributedCache _distributeCache;
    private readonly ILogger<CacheProvider> _logger;
    
    public CacheProvider(ILogger<CacheProvider> logger) {

        RedisCacheOptions redisOptions = new RedisCacheOptions{
            ConfigurationOptions = new ConfigurationOptions()
        };
        redisOptions.ConfigurationOptions.EndPoints.Add(VarcaApp.This.Configs.CacheStore.Url);
        
        var options = Options.Create<RedisCacheOptions>(redisOptions);
        _distributeCache = new RedisCache(options);
        _logger = logger;
    }

    public async Task<T> GetAsync<T>(string key) where T : class
    {
        T item = default;
        try {
            byte[] blob = await _distributeCache.GetAsync(key);
            if (blob != null & blob.Length > 0) {
                item = Deserialize<T>(blob);
            }
            if (item == null) {
                //TODO add cache missed metric
                _logger.LogWarning($"{key} : cache missed");
            }
        } catch (Exception ex) {
            _logger.LogError(ex, key);
        }
        return item;
    }

    public async Task SetAsync<T>(string key, T item) where T : class 
    {
        DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
        options.SetSlidingExpiration(TimeSpan.FromMinutes(20));
        try {
            byte[] blob = Serialize(item);
            await _distributeCache.SetAsync(key, blob);
        } catch (Exception ex) {
            _logger.LogError(ex, key);
        }
    }

    public async Task RemoveAsync(string key) {
        try {
            await _distributeCache.RemoveAsync(key);  
        } catch (Exception ex) {
            _logger.LogError(ex, key);
        }
    }

    private static T Deserialize<T>(byte[] bytes)
    {
        return JsonSerializer.Deserialize<T>(bytes)!;
    }

    private static byte[] Serialize<T>(T value)
    {
        JsonSerializerOptions options = new JsonSerializerOptions() {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        JsonSerializer.Serialize(writer, value, options);
        return buffer.WrittenSpan.ToArray();
    }
}
