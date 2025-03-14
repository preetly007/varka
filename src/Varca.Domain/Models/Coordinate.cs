using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GeoJSON.Text.Geometry;

namespace Varca.Domain.Models;


public class SearchMappingsPit 
{
       [JsonPropertyName("pitId")]
       public string PitId { get; set; }

       [JsonPropertyName("accountId")]
       public string AccountId { get; set;}

}
public class SearchMappingsGeometry 
{
       [JsonPropertyName("geometry")]
       public IGeometryObject Geometry { get; set;}

       [JsonPropertyName("filters")]
       [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
       public Filter Filters { get; set;}

       [JsonPropertyName("accountId")]
       public string AccountId { get; set;}

       [JsonPropertyName("radius")]
       public float Radius { get; set;}

       [JsonPropertyName("pitId")]
       public string PitId { get; set; }
}

public class SearchMappingsPlace
{
    [JsonPropertyName("placeId")]
    public string PlaceId { get; set; }

    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Filter Filters { get; set; } 

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; }

    [JsonPropertyName("polygon")]
    public int Polygon { get; set;}

    [JsonPropertyName("source")]
    public string Source { get; set;}

    [JsonPropertyName("radius")]
    public float Radius { get; set;}

    [JsonPropertyName("pitId")]
    public string PitId { get; set; }
}


public record ProviderPublishKey(string ProviderFamily, 
       string PublishKey, List<string> AvailableChannelIds, List<string> ApprovedChannelIds);