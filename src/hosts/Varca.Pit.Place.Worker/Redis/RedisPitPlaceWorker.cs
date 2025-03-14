using System.Text.Json;
using StackExchange.Redis;
using Varca.Domain;
using Varca.Domain.Models;
using Varca.Domain.Repositories;
using Varca.Domain.Services;

namespace Varca.Pit.Place.Worker;

public class RedisPitPlaceWorker : BackgroundService
{
    private readonly IGeoHotelMappingService _geoHotelMappingService;
    private readonly IPitResultStore _pitResultStore;
    private readonly ILogger<RedisPitPlaceWorker> _logger;

    public RedisPitPlaceWorker(ILogger<RedisPitPlaceWorker> logger,
                                    IGeoHotelMappingService geoHotelMappingService,
                                    IPitResultStore pitResultStore) {
        _geoHotelMappingService = geoHotelMappingService;
        _pitResultStore = pitResultStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var streamName = VarcaApp.This.Configs.PitStreams.PlaceDefault;
        var consumerGroupName = VarcaApp.This.Configs.PitStreams.PlaceDefaultConsumer;
        var msgKey = VarcaApp.This.Configs.PitStreams.MsgKey;
        var consumerId = Environment.GetEnvironmentVariable("VARCA_PIT_PLACE_CONSUMER_ID");

        _logger.LogInformation("Varca.Pit.Place.Worker.Redis running at: {time}", DateTimeOffset.UtcNow);
        var placeStream = VarcaApp.This.PitStreamStore.GetDatabase();
        if (!await placeStream.KeyExistsAsync(streamName).ConfigureAwait(false) 
            || (await placeStream.StreamGroupInfoAsync(streamName).ConfigureAwait(false)).All(x=>x.Name != consumerGroupName)) {
            await placeStream.StreamCreateConsumerGroupAsync(streamName, consumerGroupName, "0-0", true).ConfigureAwait(false);
        }

        var messgageId = string.Empty;
        var readPending = true;

        while (!stoppingToken.IsCancellationRequested)
        {
            try {
                if (!string.IsNullOrEmpty(messgageId)) {
                    await placeStream.StreamAcknowledgeAsync(streamName, consumerGroupName, messgageId).ConfigureAwait(false);
                    await placeStream.StreamDeleteAsync(streamName, [messgageId]).ConfigureAwait(false);
                    messgageId = string.Empty;
                }

                if (readPending) {
                    var result = await placeStream.StreamReadGroupAsync(streamName, consumerGroupName, consumerId, "0-0", 1).ConfigureAwait(false);
                    if (result.Any()) {
                        messgageId = result.First().Id;
                        var dict = ParseResult(result.First());
                        if (dict.ContainsKey(msgKey)) {
                            await DoWork(dict[msgKey]).ConfigureAwait(false);
                        }
                        _logger.LogInformation(messgageId);
                    } else {
                        readPending = false;
                    }
                }

                if (!readPending) {
                    var result = await placeStream.StreamReadGroupAsync(streamName, consumerGroupName, consumerId, ">", 1).ConfigureAwait(false);
                    if (result.Any()) {
                        messgageId = result.First().Id;
                        var dict = ParseResult(result.First());
                        if (dict.ContainsKey(msgKey)) {
                            await DoWork(dict[msgKey]).ConfigureAwait(false);
                        }
                        _logger.LogInformation(messgageId);
                    }
                }
                
            } catch (TaskCanceledException exception) {
                _logger.LogCritical(exception, exception.Message);
            }
        }
    }

    private async Task DoWork(string pitNewJson)
    {
        var pitNew = JsonSerializer.Deserialize<PitPlaceNew>(pitNewJson);
        if (pitNew != null) {
            var processStartedAtUtc = DateTimeOffset.UtcNow;
            _pitResultStore.SaveProcessStartAtAsync(pitNew.PitId, pitNew.AccountId, processStartedAtUtc).ConfigureAwait(false);

            GeoHotelMappingCollection? geoHotelMappings = default;

            var searchMappingsPlace = new SearchMappingsPlace {
                AccountId = pitNew.AccountId,
                Filters = pitNew.Filters,
                PitId = pitNew.PitId,
                PlaceId = pitNew.PlaceId,
                Polygon = pitNew.Polygon,
                Radius = pitNew.Radius,
                Source = pitNew.Source
            };

            geoHotelMappings = await _geoHotelMappingService.SearchByPlaceAsync(searchMappingsPlace).ConfigureAwait(false);
            await _pitResultStore.SaveMappingsAsync(pitNew.AccountId, pitNew.PitId, geoHotelMappings).ConfigureAwait(false);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Varca.Pit.Place.Worker.Redis is stopping.");
        await base.StopAsync(stoppingToken).ConfigureAwait(false);
    }

    static Dictionary<string, string> ParseResult(StreamEntry entry) => 
        entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
}

