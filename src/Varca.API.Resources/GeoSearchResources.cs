using System.Text.Json.Serialization;
using GeoJSON.Text.Geometry;

namespace Varca.API.Resources;

 
public class SearchGeometryResource 
{
       [JsonPropertyName("geometry")]
       public IGeometryObject Geometry { get; set;}

       [JsonPropertyName("filters")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public FilterResource? Filters { get; set;}
}

public class SearchPlaceResource
{
       [JsonPropertyName("filters")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public FilterResource? Filters { get; set;}
}


public class MappingResource
{
       [JsonPropertyName("vervotechId")]
       public int VervotechId { get; set; }
       
       [JsonPropertyName("providerHotelId")]
       public string ProviderHotelId { get; set; }

       [JsonPropertyName("providerName")]
       public string ProviderName { get; set; }
       
       [JsonPropertyName("channelIds")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public List<string> ChannelIds { get; set; }

       [JsonPropertyName("providerLocationCode")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public string ProviderLocationCode { get; set; }

       [JsonPropertyName("geoLocation")]
       public Point GeoLocation { get; set; }

       [JsonPropertyName("rating")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public string Rating { get; set;}

       [JsonPropertyName("chainCode")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public string ChainCode { get; set;}

       [JsonPropertyName("propertyType")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public string PropertyType { get; set;}
}


public class MappingCollectionResource
{
       [JsonPropertyName("mappings")]
       public List<MappingResource> Mappings { get; set; }
}




