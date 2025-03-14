using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Varca.Domain.Models;

public class PlaceKeywordSearchQuery
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; }

    [JsonPropertyName("keyword")]
    public string Keyword { get; set; }

    [JsonPropertyName("source")]
    public GeoPlaceSource Source { get; set; }

    [JsonPropertyName("limit")]
    public int Limit { get; set; }
}

public class PlaceIdQuery 
{
    [JsonPropertyName("accountId")]
    public string AccountId { get; set; }

    [JsonPropertyName("placeId")]
    public string PlaceId { get; set; }

    [JsonPropertyName("source")]
    public GeoPlaceSource Source { get; set; }
}

