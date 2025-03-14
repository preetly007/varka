using Amazon.Runtime.Internal.Util;
using GeoJSON.Text;
using Microsoft.Extensions.Logging;
using Varca.Domain.Models;
using Varca.Domain.Repositories;

namespace Varca.Domain.Services;

public class GeoHotelMappingService : IGeoHotelMappingService
{
    private readonly IGeoHotelMappingStoreQuery _geoHotelMappingStoreQuery;
    private readonly IPlaceSearchService _placeSearchService;
    private readonly IPitResultStore _pitResultStore;
    private readonly IAccountService _accountService;
    private readonly ILogger<GeoHotelMappingService> _logger;
    
    public GeoHotelMappingService(ILogger<GeoHotelMappingService> logger,
                                    IGeoHotelMappingStoreQuery geoHotelMappingStoreQuery,
                                    IAccountService accountService,
                                    IPitResultStore pitResultStore,
                                    IPlaceSearchService placeSearchService) {
        _geoHotelMappingStoreQuery = geoHotelMappingStoreQuery;
        _accountService = accountService;
        _pitResultStore = pitResultStore;
        _placeSearchService = placeSearchService;
        _logger = logger;
    }

    public async Task<GeoHotelMappingCollection?> SearchByMultipolygonAsync(SearchMappingsGeometry searchMultiPolygon)
    {
        if (string.IsNullOrEmpty(searchMultiPolygon.PitId)) {
            searchMultiPolygon = await _accountService.BuildAccessKeysFilterAsync(searchMultiPolygon).ConfigureAwait(false);
            if (searchMultiPolygon.Filters.AccessKeys.Count == 0) {
                return null;
            }
        }
        var mappings = await _geoHotelMappingStoreQuery.SearchByMultiPolygonAsync(searchMultiPolygon).ConfigureAwait(false);
        return mappings;
        
    }

    public async Task<GeoHotelMappingCollection?> SearchByPitAsync(SearchMappingsPit searchQuery)
    {
        var geoHotelMappings = await _pitResultStore.GetMappingsAsync(searchQuery.AccountId, searchQuery.PitId);
        return geoHotelMappings;
    }

    public async Task<GeoHotelMappingCollection?> SearchByPlaceAsync(SearchMappingsPlace searchPlace)
    {
        GeoHotelMappingCollection? mappings = default;

        var searchPlaceIdQuery = new PlaceIdQuery{
            AccountId = searchPlace.AccountId,
            PlaceId = searchPlace.PlaceId,
            Source = GeoPlaceSource.Vervotech
        };

        var feature = await _placeSearchService.GetByPlaceIdAsync(searchPlaceIdQuery).ConfigureAwait(false);
        if (feature == null) {
            return mappings;
        }

        if (string.IsNullOrEmpty(searchPlace.PitId)) {
            searchPlace = await _accountService.BuildAccessKeysFilterAsync(searchPlace).ConfigureAwait(false);
            if (searchPlace.Filters.AccessKeys.Count == 0) {
                return mappings;
            }
        }

        var searchMappingsGeometry = new SearchMappingsGeometry {
            AccountId = searchPlace.AccountId,
            Filters = searchPlace.Filters,
            PitId = searchPlace.PitId,
            Radius = searchPlace.Radius,
            Geometry = feature.Geometry
        };

        //if polygon = 0, fallback to point
        //feature.Properties.TryGetValue("")

        if (feature.Geometry.Type == GeoJSONObjectType.Point) {
            mappings = await _geoHotelMappingStoreQuery.SearchByPointAsync(searchMappingsGeometry).ConfigureAwait(false);
        } else if (feature.Geometry.Type == GeoJSONObjectType.Polygon) {
            mappings = await _geoHotelMappingStoreQuery.SearchByPolygonAsync(searchMappingsGeometry).ConfigureAwait(false);
        } else if (feature.Geometry.Type == GeoJSONObjectType.MultiPolygon) {
            mappings = await _geoHotelMappingStoreQuery.SearchByMultiPolygonAsync(searchMappingsGeometry).ConfigureAwait(false);
        } 

        return mappings;
    }

    public async Task<GeoHotelMappingCollection?> SearchByPointAsync(SearchMappingsGeometry searchPoint)
    {
        if (string.IsNullOrEmpty(searchPoint.PitId)) {
            searchPoint = await _accountService.BuildAccessKeysFilterAsync(searchPoint).ConfigureAwait(false);
            if (searchPoint.Filters.AccessKeys.Count == 0) {
                return null;
            }
        }
        var mappings = await _geoHotelMappingStoreQuery.SearchByPointAsync(searchPoint).ConfigureAwait(false);
        return mappings;
    }

    public async Task<GeoHotelMappingCollection?> SearchByPolygonAsync(SearchMappingsGeometry searchPolygon)
    {
        if (string.IsNullOrEmpty(searchPolygon.PitId)) {
            searchPolygon = await _accountService.BuildAccessKeysFilterAsync(searchPolygon).ConfigureAwait(false);
            if (searchPolygon.Filters.AccessKeys.Count == 0) {
                return null;
            }
        }
        var mappings = await _geoHotelMappingStoreQuery.SearchByPolygonAsync(searchPolygon).ConfigureAwait(false);
        return mappings;
    }
}
