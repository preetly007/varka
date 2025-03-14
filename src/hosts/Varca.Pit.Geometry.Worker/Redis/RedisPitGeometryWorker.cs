using System.Text.Json;
using StackExchange.Redis;
using Varca.Domain;
using Varca.Domain.Models;
using Varca.Domain.Repositories;
using Varca.Domain.Services;

namespace Varca.Pit.Geometry.Worker.Redis;


public class RedisPitGeometryWorker : BackgroundService
{
    private readonly IGeoHotelMappingService _geoHotelMappingService;
    private readonly IPitResultStore _pitResultStore;
    private readonly ILogger<RedisPitGeometryWorker> _logger;
    public RedisPitGeometryWorker(ILogger<RedisPitGeometryWorker> logger,
                                    IGeoHotelMappingService geoHotelMappingService,
                                    IPitResultStore pitResultStore) {
        _geoHotelMappingService = geoHotelMappingService;
        _pitResultStore = pitResultStore;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var streamName = VarcaApp.This.Configs.PitStreams.GeometryDefault;
        var consumerGroupName = VarcaApp.This.Configs.PitStreams.GeometryDefaultConsumer;
        var msgKey = VarcaApp.This.Configs.PitStreams.MsgKey;
        var consumerId = Environment.GetEnvironmentVariable("VARCA_PIT_GEOMETRY_CONSUMER_ID");

        _logger.LogInformation("Varca.Pit.Geometry.Worker.Redis running at: {time}", DateTimeOffset.UtcNow);

        var geometryStream = VarcaApp.This.PitStreamStore.GetDatabase();
        if (!await geometryStream.KeyExistsAsync(streamName).ConfigureAwait(false) 
            || (await geometryStream.StreamGroupInfoAsync(streamName).ConfigureAwait(false)).All(x=>x.Name != consumerGroupName)) {
            await geometryStream.StreamCreateConsumerGroupAsync(streamName, consumerGroupName, "0-0", true).ConfigureAwait(false);
        }

        var messgageId = string.Empty;
        var readPending = true;

        while (!stoppingToken.IsCancellationRequested)
        {
            try {
                if (!string.IsNullOrEmpty(messgageId)) {
                    await geometryStream.StreamAcknowledgeAsync(streamName, consumerGroupName, messgageId).ConfigureAwait(false);
                    await geometryStream.StreamDeleteAsync(streamName, [messgageId]).ConfigureAwait(false);
                    messgageId = string.Empty;
                }

                if (readPending) {
                    var result = await geometryStream.StreamReadGroupAsync(streamName, consumerGroupName, consumerId, "0-0", 1).ConfigureAwait(false);
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
                    var result = await geometryStream.StreamReadGroupAsync(streamName, consumerGroupName, consumerId, ">", 1).ConfigureAwait(false);
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
        var pitNew = JsonSerializer.Deserialize<PitGeometryNew>(pitNewJson);
        if (pitNew != null) {
            var processStartedAtUtc = DateTimeOffset.UtcNow;
            _pitResultStore.SaveProcessStartAtAsync(pitNew.PitId, pitNew.AccountId, processStartedAtUtc).ConfigureAwait(false);

            GeoHotelMappingCollection? geoHotelMappings;

            if (pitNew.Geometry.Type == GeoJSON.Text.GeoJSONObjectType.Point) {
                var searchMappingsGeometry = new SearchMappingsGeometry
                {
                    PitId = pitNew.PitId,
                    AccountId = pitNew.AccountId,
                    Filters = pitNew.Filters,
                    Geometry = pitNew.Geometry,
                    Radius = pitNew.Radius
                };
                geoHotelMappings = await _geoHotelMappingService.SearchByPointAsync(searchMappingsGeometry).ConfigureAwait(false);
                await _pitResultStore.SaveMappingsAsync(pitNew.AccountId, pitNew.PitId, geoHotelMappings).ConfigureAwait(false);
            } 
            else if (pitNew.Geometry.Type == GeoJSON.Text.GeoJSONObjectType.Polygon) {
                var searchMappingsGeometry = new SearchMappingsGeometry {
                    PitId = pitNew.PitId,
                    AccountId = pitNew.AccountId,
                    Filters = pitNew.Filters,
                    Geometry = pitNew.Geometry
                };
                geoHotelMappings = await _geoHotelMappingService.SearchByPolygonAsync(searchMappingsGeometry).ConfigureAwait(false);
                await _pitResultStore.SaveMappingsAsync(pitNew.AccountId, pitNew.PitId, geoHotelMappings).ConfigureAwait(false);
            } 
            else if (pitNew.Geometry.Type == GeoJSON.Text.GeoJSONObjectType.MultiPolygon) {
                var searchMappingsGeometry = new SearchMappingsGeometry {
                    PitId = pitNew.PitId,
                    AccountId = pitNew.AccountId,
                    Filters = pitNew.Filters,
                    Geometry = pitNew.Geometry
                };
                geoHotelMappings = await _geoHotelMappingService.SearchByMultipolygonAsync(searchMappingsGeometry).ConfigureAwait(false);
                await _pitResultStore.SaveMappingsAsync(pitNew.AccountId, pitNew.PitId, geoHotelMappings).ConfigureAwait(false);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Varca.Pit.Geometry.Worker.Redis is stopping.");
        await base.StopAsync(stoppingToken).ConfigureAwait(false);
    }

    static Dictionary<string, string> ParseResult(StreamEntry entry) => 
        entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
}
