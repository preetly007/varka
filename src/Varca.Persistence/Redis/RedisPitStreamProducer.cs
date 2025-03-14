using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Varca.Domain.Models;
using Varca.Domain.Repositories;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Varca.Domain;


namespace Varca.Persistence.Redis;

public class RedisPitStreamProducer : IPitStreamProducer
{
    private readonly string geometryStreamName = VarcaApp.This.Configs.PitStreams.GeometryDefault;
    private readonly string placeStreamName = VarcaApp.This.Configs.PitStreams.PlaceDefault;
    private readonly string messageKey = VarcaApp.This.Configs.PitStreams.MsgKey;
    private readonly ILogger<RedisPitStreamProducer> _logger;
    public RedisPitStreamProducer(ILogger<RedisPitStreamProducer> logger) {
        _logger = logger;
    }
    public async Task<PitGeometryNew?> SendGeometryAsync(PitGeometryNew pitNew)
    {
        var message = JsonSerializer.Serialize(pitNew);
        var size = message.Length;

        var redisDB = VarcaApp.This.PitStreamStore.GetDatabase();
        var id = await redisDB.StreamAddAsync(geometryStreamName, messageKey, message, maxLength: size, useApproximateMaxLength:true);
        if (id.HasValue) {
            pitNew.StreamMessageId = id.ToString();
        } else {
            return null;
        }
        return pitNew;
    }

    public async Task<PitPlaceNew?> SendPlaceAsync(PitPlaceNew pitNew)
    {
        var message = JsonSerializer.Serialize(pitNew);
        var size = message.Length;
        
        var redisDB = VarcaApp.This.PitStreamStore.GetDatabase();
        var id = await redisDB.StreamAddAsync(placeStreamName, messageKey, message, maxLength: size, useApproximateMaxLength:true);
        if (id.HasValue) {
            pitNew.StreamMessageId = id.ToString();
        } else {
            return null;
        }
        return pitNew;
    }
} 
