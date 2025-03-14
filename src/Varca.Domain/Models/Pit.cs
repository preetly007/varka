using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GeoJSON.Text.Geometry;

namespace Varca.Domain.Models;

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum PitContext
{
    HotelMappings,
    Content
}
public class PitGeometry
{
    [JsonPropertyName("contexts")]
    public List<PitContext> Contexts { get; set; }

    [JsonPropertyName("geometry")]
    public IGeometryObject Geometry { get; set;}

    [JsonPropertyName("optionalFields")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> OptionalFields { get; set;}

    [JsonPropertyName("filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Filter Filters { get; set;}

    [JsonPropertyName("accountId")]
    public string AccountId { get; set;}

    [JsonPropertyName("radius")]
    public float Radius { get; set;}

    [JsonPropertyName("streamMessageId")]
    public string StreamMessageId { get; set;}
}

public class PitPlace
{
    [JsonPropertyName("contexts")]
    public List<PitContext> Contexts { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("optionalFields")]
    public List<string> OptionalFields { get; set;}

    [JsonPropertyName("filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Filter Filters { get; set;}

    [JsonPropertyName("accountId")]
    public string AccountId { get; set;}

    [JsonPropertyName("radius")]
    public float Radius { get; set;}

    [JsonPropertyName("polygon")]
    public int Polygon { get; set;}

    [JsonPropertyName("placeId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string PlaceId { get; set;}

    [JsonPropertyName("source")]
    public string Source { get; set;}

    [JsonPropertyName("streamMessageId")]
    public string StreamMessageId { get; set;}
}

public class Filter
{
    [JsonPropertyName("providers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ProviderFilter> Providers { get; set; }

    [JsonPropertyName("ratings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> Ratings { get; set;}

    [JsonPropertyName("chainCodes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> ChainCodes { get; set;}

    [JsonPropertyName("propertyTypes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> PropertyTypes { get; set; }

    [JsonPropertyName("accessKeys")]
    public List<string> AccessKeys { get; set; }
}

public class ProviderFilter
{
    [JsonPropertyName("name")]
    public string Name { get; set;}

    [JsonPropertyName("channelIds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string> ChannelIds { get; set;}
}


public class PitGeometryNew : PitGeometry
{
    [JsonPropertyName("pitId")]
    public string PitId { get; set; }

    [JsonPropertyName("createdAtUtc")]
    public DateTimeOffset CreatedAtUtc { get; set;}

    [JsonPropertyName("ttl")]
    public int TTL { get; set;}
}

public class PitPlaceNew : PitPlace
{
    [JsonPropertyName("pitId")]
    public string PitId { get; set; }

    [JsonPropertyName("createdAtUtc")]
    public DateTimeOffset CreatedAtUtc { get; set;}

    [JsonPropertyName("ttl")]
    public int TTL { get; set;}
}

public class PitInfo 
{
    [JsonPropertyName("pitId")]
    public string PitId { get; set; }

    [JsonPropertyName("contexts")]
    public List<PitContext> Contexts { get; set; }

    [JsonPropertyName("createdAtUtc")]
    public DateTimeOffset CreatedAtUtc { get; set;}

    [JsonPropertyName("ttl")]
    public int TTL { get; set;}

    [JsonPropertyName("hotelMappingsAvailableAtUtc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? HotelMappingsAvailableAtUtc { get; set;}

    [JsonPropertyName("contentAvailableAtUtc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? ContentAvailableAtUtc { get; set;}

    [JsonPropertyName("processStartedAtUtc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? ProcessStartedAtUtc { get; set;}
}