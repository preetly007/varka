using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GeoJSON.Text.Geometry;

namespace Varca.API.Resources;

[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum PitContextResource
{
    HotelMappings,
    Content
}

public class PitGeometryResource
{
    [JsonPropertyName("contexts")]
    public List<PitContextResource> Contexts { get; set; }

    [JsonPropertyName("geometry")]
    public IGeometryObject Geometry { get; set;}

    [JsonPropertyName("optionalFields")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? OptionalFields { get; set;}

    [JsonPropertyName("filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FilterResource? Filters { get; set;}
}

public class PitPlaceResource 
{
    [JsonPropertyName("contexts")]
    public List<PitContextResource> Contexts { get; set; }

    [JsonPropertyName("optionalFields")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? OptionalFields { get; set;}

    [JsonPropertyName("filters")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FilterResource? Filters { get; set;}
}

public class FilterResource 
{
       [JsonPropertyName("providers")]
       public List<ProviderResource>? Providers { get; set; }

       [JsonPropertyName("ratings")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public List<string>? Ratings { get; set;}
       
       [JsonPropertyName("chainCodes")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public List<string>? ChainCodes { get; set;}

       [JsonPropertyName("propertyTypes")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public List<string>? PropertyTypes { get; set;}
}

public class ProviderResource 
{
    [JsonPropertyName("name")]    
    public string Name { get; set;}

    [JsonPropertyName("channelIds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ChannelIds { get; set;}
}

public class PitNewResouce 
{
    [JsonPropertyName("pitId")]
    public string PitId { get; set; }

    [JsonPropertyName("contexts")]
    public List<PitContextResource> Contexts { get; set; }

    [JsonPropertyName("createdAtUtc")]
    public DateTimeOffset CreatedAtUtc { get; set;}

    [JsonPropertyName("ttl")]
    public int TTL { get; set;}
}

public class PitInfoResouce 
{
    [JsonPropertyName("pitId")]
    public string PitId { get; set; }

    [JsonPropertyName("contexts")]
    public List<PitContextResource> Contexts { get; set; }

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