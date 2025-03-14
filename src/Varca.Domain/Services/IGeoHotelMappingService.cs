using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unica.Core.Contracts.Models;
using Varca.Domain.Models;

namespace Varca.Domain.Services;

public interface IGeoHotelMappingService
{
    Task<GeoHotelMappingCollection?> SearchByPointAsync(SearchMappingsGeometry pointQuery);
    Task<GeoHotelMappingCollection?> SearchByPolygonAsync(SearchMappingsGeometry polygonQuery);
    Task<GeoHotelMappingCollection?> SearchByMultipolygonAsync(SearchMappingsGeometry multipolygonQuery);
    Task<GeoHotelMappingCollection?> SearchByPlaceAsync(SearchMappingsPlace placeQuery);
    Task<GeoHotelMappingCollection?> SearchByPitAsync(SearchMappingsPit searchQuery);
}

