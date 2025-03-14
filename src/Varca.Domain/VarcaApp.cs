
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using MongoDB.Driver;
using StackExchange.Redis;
using Varca.Domain.Models;

namespace Varca.Domain;

public sealed class VarcaApp : IDisposable
{
    public Config Configs { get; }
    public MongoClient UnicaReadOnlyStore { get; }
    public MongoClient GeoHotelMappingsStore { get; }
    public MongoClient ContentStore { get; }
    //public ConnectionMultiplexer CacheStore { get; }
    public ConnectionMultiplexer PitStreamStore { get; }
    public ElasticsearchClient PlaceStore { get; }

    private bool _disposed = false;

    public VarcaApp(string varcaUberConfigFile) {

        Configs = Config.Load(varcaUberConfigFile);

        var nodes = new Uri[Configs.PlaceStore.Nodes.Count];
        for (int index = 0; index < Configs.PlaceStore.Nodes.Count; index++) {
            nodes[index] = new Uri(Configs.PlaceStore.Nodes[index]);
        }

        var pool = new StaticNodePool(nodes);
        var settings = new ElasticsearchClientSettings(pool);
            //Elastic 8 feature
            //.CertificateFingerprint(Configs.PlaceStore.Ca)
            //.Authentication(new ApiKey(Configs.PlaceStore.Apikey));
        PlaceStore = new ElasticsearchClient(settings);

        UnicaReadOnlyStore = new MongoClient(Configs.UnicaDbReadOnly.Url);
        
        var mongoClient = new MongoClient(Configs.GeoStore.Url);
        GeoHotelMappingsStore = mongoClient;
        ContentStore = mongoClient;

        var redisMultiPlexer = ConnectionMultiplexer.Connect(Configs.PitStreamStore.Url);
        PitStreamStore = redisMultiPlexer;

        This = this;
    }

    public async void Dispose()
    {
        if (!_disposed) {
            try {
                await PitStreamStore.CloseAsync().ConfigureAwait(false);
                //var t2 = CacheStore.CloseAsync();
                //await Task.WhenAll(t1, t2);
            } catch {}
        }
        _disposed = true;
    }

    public static VarcaApp This { get; private set; }
}