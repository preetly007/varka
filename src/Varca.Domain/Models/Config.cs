using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Varca.Domain.Models;

public class Config
{
    public GeoStoreConfig GeoStore { get; set; }
    public ContentStoreConfig ContentStore { get; set; }
    public PlaceStoreConfig PlaceStore { get; set; }
    public CacheStoreConfig CacheStore {get; set; }
    public PitStreamStoreConfig PitStreamStore { get; set; }
    public pitStreamsConfig PitStreams { get; set; }
    public UnicaDbReadOnlyConfig UnicaDbReadOnlyMigration { get; set; }
    public UnicaDbReadOnlyConfig UnicaDbReadOnly { get; set; }
    public VarcaServicesConfig VarcaServices { get; set; }
    public ServicesConfig Services { get; set; }
    
    public static Config Load(string fileName) {

        using var reader = new StreamReader(fileName);
        var ymlText = reader.ReadToEnd();
        
        var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

        var config = deserializer.Deserialize<Config>(ymlText);
        return config;
    }
}

public class GeoStoreConfig {
    public string Type { get; set; }
    public string Url { get; set; }
    public string Db { get; set; }
    public string GhmsCollection { get; set; }
    public string GhmsIdx { get; set; }
}

public class ContentStoreConfig {
    public string Type { get; set; }
    public string Url { get; set; }
    public string Db { get; set; }
    public string ContentCollection { get; set; }
    public string ContentIdx { get; set; }
}

public class PlaceStoreConfig {
    public string Type { get; set; }
    public List<string> Nodes { get; set; }
    public string Apikey { get; set; }
    public string Ca { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }
    public string PlaceVervotechIdx { get; set; }
    public string PlaceEanIdx { get; set; }
}

public class CacheStoreConfig {
  public string Type { get; set; }
  public string Url { get; set; }
}

public class PitStreamStoreConfig {
  public string Type { get; set; }
  public string Url { get; set; }
}

public class pitStreamsConfig {
    public string Type { get; set; }
    public string MsgKey { get; set; }
    public string GeometryDefault { get; set; }
    public string GeometryDefaultConsumer { get; set; }
    public string PlaceDefault { get; set; }
    public string PlaceDefaultConsumer { get; set; }
    public string GeometryPriorities { get; set; }
    public string PlacePriorities { get; set; }
}

public class UnicaDbReadOnlyConfig {
    public string Type { get; set; }
    public string Url { get; set; }
    public string UnicaConfigDb { get; set; }
    public string UnicaProvidersCollection { get; set; }
}

public class VarcaServicesConfig {
    public VarcaServiceConfig Place { get; set; }
    public VarcaServiceConfig Ghms { get; set; }
    public VarcaServiceConfig Pit { get; set; }
    public VarcaServiceConfig Content { get; set; }
}

public class VarcaServiceConfig {
    public string Address { get; set; }
    public int Port { get; set; }
}

public class ServicesConfig {
    public ServiceConfig Osm { get; set; }
}
public class ServiceConfig {
    public string EndPoint { get; set; }
}


public class ShoeboxesConfig {
    public string Type { get; set; }
    public string EndPoint { get; set; }
    public ShoeBoxConfig GhmsShoebox { get; set; }
    public ShoeBoxConfig PlaceShoebox { get; set; }
    public ShoeBoxConfig ScorerShoebox { get; set; }
}

public class ShoeBoxConfig {
    public string SourceQueue { get; set; }
    public string DestinationExchange { get; set; }
}