using System.Text.Json.Serialization;

namespace Varca.Domain.Models;


[JsonConverter(typeof(JsonStringEnumConverter))] 
public enum GeoPlaceSource
{
    Vervotech,

    EAN
}
