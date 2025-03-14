using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Varca.Domain.Models;

namespace Varca.Domain.Repositories;

public interface IGeoHotelMappingStoreQuery
{
    Task<GeoHotelMappingCollection?> SearchByPolygonAsync(SearchMappingsGeometry searchPolygon);
    Task<GeoHotelMappingCollection?> SearchByPointAsync(SearchMappingsGeometry searchPoint);
    Task<GeoHotelMappingCollection?> SearchByMultiPolygonAsync(SearchMappingsGeometry searchMultiPolygon);
}
