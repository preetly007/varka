using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Varca.Domain.Models;
using Varca.Domain.Repositories;

namespace Varca.Domain.Services;

public class RedisPitResultStore : IPitResultStore
{
    private readonly ILogger<RedisPitResultStore> _logger;
    private const string _msgProcessStartedAtKey = "process-started-at";
    private const string _msgHmsAvailableAtKey = "hms-available-at";
    private const string _msgContentAvailableAtKey = "content-available-at";
    private const string _msgHmsKey = "hms";
    private const string _msgConentKey = "content";
    private const string _mgsPitInfo = "pitinfo";

    public RedisPitResultStore(ILogger<RedisPitResultStore> logger) {
        _logger = logger;
    }
    public async Task GetContentAsync(string accountId, string pitId)
    {
        throw new NotImplementedException();
    }

    public async Task<GeoHotelMappingCollection?> GetMappingsAsync(string accountId, string pitId)
    {
        var redisDb = VarcaApp.This.PitStreamStore.GetDatabase();
        var key = GetPitResultKey(accountId, pitId);

        var resultValue = await redisDb.HashGetAsync(key, _msgHmsKey);
        if (resultValue.HasValue) {
            if (resultValue.Equals("0")) {
                return new GeoHotelMappingCollection();
            } else {
                var mappingsJson = resultValue.ToString();
                var mappings = JsonSerializer.Deserialize<GeoHotelMappingCollection>(mappingsJson);
                return mappings;
            }
        } else {
            return null;
        }
    }

    public async Task SaveMappingsAsync(string accountId, string pitId, GeoHotelMappingCollection? mappings)
    {
        var redisDb = VarcaApp.This.PitStreamStore.GetDatabase();
        var key = GetPitResultKey(accountId, pitId);
        var hashEntries = new List<HashEntry>();

        var mappingsAvailableAtUtc = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        hashEntries.Add(new HashEntry(_msgHmsAvailableAtKey, mappingsAvailableAtUtc));
        
        if (mappings != null) {
            var mappingsJson = JsonSerializer.Serialize(mappings);
            hashEntries.Add(new HashEntry(_msgHmsKey, mappingsJson));
        }
        
        var length = await redisDb.HashLengthAsync(key);
        if (length > 0)
            await redisDb.HashSetAsync(key, [.. hashEntries]);
    }

    public async Task SaveContentAsync(string accountId, string pitId)
    {
        var redisDb = VarcaApp.This.PitStreamStore.GetDatabase();
        var key = GetPitResultKey(accountId, pitId);

        var contentJson = 0; //JsonSerializer.Serialize(mappings);
        var contentAvailableAtUtc = 0; //DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var hashEntries = new HashEntry[] {
            new HashEntry(_msgConentKey, contentJson),
            new HashEntry(_msgContentAvailableAtKey, contentAvailableAtUtc)
        };

        var length = await redisDb.HashLengthAsync(key);
        if (length > 0)
            await redisDb.HashSetAsync(key, hashEntries);
    }
    
    public async Task<PitPlaceNew?> SavePlaceAsync(PitPlaceNew pitNew)
    {
        var redisDb = VarcaApp.This.PitStreamStore.GetDatabase();
        var key = GetPitResultKey(pitNew.AccountId, pitNew.PitId);

        var pitInfo = new PitInfo
        {
            Contexts = pitNew.Contexts,
            CreatedAtUtc = pitNew.CreatedAtUtc,
            PitId = pitNew.PitId,
            TTL = pitNew.TTL,
            ContentAvailableAtUtc = null,
            HotelMappingsAvailableAtUtc = null,
            ProcessStartedAtUtc = null
        };
        var pitInfoJson = JsonSerializer.Serialize(pitInfo);

        var hashEntries = new HashEntry[] {
            new HashEntry(_mgsPitInfo, pitInfoJson),
            new HashEntry(_msgHmsKey, 0),
            new HashEntry(_msgHmsAvailableAtKey, 0),
            new HashEntry(_msgConentKey, 0),
            new HashEntry(_msgContentAvailableAtKey, 0),
            new HashEntry(_msgProcessStartedAtKey, 0)
        };

        await redisDb.HashSetAsync(key, hashEntries);
        await redisDb.KeyExpireAsync(key, TimeSpan.FromMinutes(pitNew.TTL), ExpireWhen.Always);
        return pitNew;
    }

    public async Task<PitGeometryNew?> SaveGeometryAsync(PitGeometryNew pitNew)
    {
        var redisDb = VarcaApp.This.PitStreamStore.GetDatabase();
        var key = GetPitResultKey(pitNew.AccountId, pitNew.PitId);

        var pitInfo = new PitInfo
        {
            Contexts = pitNew.Contexts,
            CreatedAtUtc = pitNew.CreatedAtUtc,
            PitId = pitNew.PitId,
            TTL = pitNew.TTL,
            ContentAvailableAtUtc = null,
            HotelMappingsAvailableAtUtc = null,
            ProcessStartedAtUtc = null
        };
        var pitInfoJson = JsonSerializer.Serialize(pitInfo);

        var hashEntries = new HashEntry[] {
            new HashEntry(_mgsPitInfo, pitInfoJson),
            new HashEntry(_msgHmsKey, 0),
            new HashEntry(_msgHmsAvailableAtKey, 0),
            new HashEntry(_msgConentKey, 0),
            new HashEntry(_msgContentAvailableAtKey, 0),
            new HashEntry(_msgProcessStartedAtKey, 0)
        };

        await redisDb.HashSetAsync(key, hashEntries);
        await redisDb.KeyExpireAsync(key, TimeSpan.FromMinutes(pitNew.TTL), ExpireWhen.Always);
        return pitNew;
    }

    public static string GetPitResultKey(string accountId, string pitId) {
        return $"varca-pit-{accountId}-{pitId}";
    }

    public async Task<PitInfo?> GetByIdAsync(string pitId, string accountId)
    {
        var redisDb = VarcaApp.This.PitStreamStore.GetDatabase();
        var key = GetPitResultKey(accountId, pitId);

        var results = await redisDb.HashGetAsync(key, [_mgsPitInfo, _msgHmsAvailableAtKey, _msgContentAvailableAtKey, _msgProcessStartedAtKey]);
        if (results.Length == 4) {
            if (!results[0].HasValue) {
                return null;
            }

            var pitInfo = JsonSerializer.Deserialize<PitInfo>(results[0].ToString());
            if (pitInfo != null) {
                if (results[1].HasValue && (long)results[1] > 0) 
                    pitInfo.HotelMappingsAvailableAtUtc = DateTimeOffset.FromUnixTimeMilliseconds((long)results[1]);
                if (results[2].HasValue && (long)results[2] > 0)
                    pitInfo.ContentAvailableAtUtc = DateTimeOffset.FromUnixTimeMilliseconds((long)results[2]);
                if (results[3].HasValue && (long)results[3] > 0)
                    pitInfo.ProcessStartedAtUtc = DateTimeOffset.FromUnixTimeMilliseconds((long)results[3]);
            }
            return pitInfo;
        }
        return null;
    }

    public async Task SaveProcessStartAtAsync(string pitId, string accountId, DateTimeOffset processStartedAtUtc) {
        var redisDb = VarcaApp.This.PitStreamStore.GetDatabase();
        var key = GetPitResultKey(accountId, pitId);
        await redisDb.HashSetAsync(key, _msgProcessStartedAtKey, processStartedAtUtc.ToUnixTimeMilliseconds());
    }
}
