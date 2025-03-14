using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GeoJSON.Text.Geometry;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Varca.Domain.Models;

[BsonIgnoreExtraElements]
public class GeoHotelMapping 
{
    [JsonPropertyName("publishKey")]
    [BsonElement("publishKey")]
    public string PublishKey { get; set; }

    [JsonPropertyName("accessKeys")]
    [BsonElement("accessKeys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> AccessKeys { get; set; }

    [JsonPropertyName("id")]
    [BsonElement("_id")]
    [BsonId]
    public string Id { get; set; }

    [JsonPropertyName("unicaId")]
    [BsonElement("unicaId")]
    public int UnicaId { get; set; }
    
    [JsonPropertyName("providerHotelId")]
    [BsonElement("providerHotelId")]
    public string ProviderHotelId { get; set; }

    [JsonPropertyName("providerFamily")]
    [BsonElement("providerFamily")]
    public string ProviderFamily { get; set; }
    
    [JsonPropertyName("channelIds")]
    [BsonElement("channelIds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> ChannelIds { get; set; }

    [JsonPropertyName("propertyType")]
    [BsonElement("propertyType")]
    public string PropertyType { get; set; }

    [JsonPropertyName("geoLocation")]
    [BsonElement("geoLocation")]
    [BsonSerializer(typeof(PointSerializer))]
    public Point GeoLocation { get; set; }

    [JsonPropertyName("lastUpdatedAt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonElement("lastUpdatedAt")]
    public DateTime? LastUpdatedAt { get; set; }

    [JsonPropertyName("providerLocationCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonElement("providerLocationCode")]
    public string ProviderLocationCode { get; set; }

    [JsonPropertyName("runId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonElement("runId")]
    public string RunId { get; set; }

    [JsonPropertyName("countryCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonElement("countryCode")]
    public string CountryCode { get; set; }

    [JsonPropertyName("rating")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonElement("rating")]
    public string Rating { get; set; }

    [JsonPropertyName("chainCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonElement("chainCode")]
    public string ChainCode { get; set; }
    public static string FormatAccessKey(string publishKey, string channelId) {
        var accessKey = $"{publishKey}__{channelId}".ToLower();
        return accessKey;
    }
}

[BsonIgnoreExtraElements]
public class GeoHotelMappingCollection
{
    [JsonPropertyName("mappings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonElement("mappings")]
    public List<GeoHotelMapping> Mappings { get; set; }

    
}

public class PointSerializer : SerializerBase<Point>
{
    public override Point Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bson = BsonDocumentSerializer.Instance.Deserialize(context);
        return JsonSerializer.Deserialize<Point>(bson.ToJson()); 
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Point value)
    {
        if (value != null) {
            var json = JsonSerializer.Serialize<Point>(value); 
            BsonDocumentSerializer.Instance.Serialize(context, args, BsonDocument.Parse(json));
        }
    }
}