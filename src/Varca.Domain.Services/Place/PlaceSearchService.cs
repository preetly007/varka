using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoJSON.Text.Feature;
using Microsoft.Extensions.Logging;
using Unica.Core.Contracts;
using Varca.Domain.Models;
using Varca.Domain.Repositories;

namespace Varca.Domain.Services;

public class PlaceSearchService : IPlaceSearchService
{
    private readonly IPlaceStoreQuery _placeStoreQuery;
    private readonly ILogger<PlaceSearchService> _logger;

    public PlaceSearchService(ILogger<PlaceSearchService> logger, IPlaceStoreQuery  placeStoreQuery) {
        _placeStoreQuery = placeStoreQuery;
        _logger = logger;
    } 
    public async Task<Feature?> GetByPlaceIdAsync(PlaceIdQuery placeIdQuery)
    {
        Feature? feature = default;
        if (placeIdQuery.Source == GeoPlaceSource.EAN) {
            return feature;
            //check account has access to EAN
        }
        feature = await _placeStoreQuery.GetByPlaceId(placeIdQuery);
        return feature;
    }

    public async Task<FeatureCollection?> SearchByKeywordAsync(PlaceKeywordSearchQuery placeKeywordSearchQuery)
    {
        FeatureCollection? featureCollection = default;
        if (placeKeywordSearchQuery.Source == GeoPlaceSource.EAN) {
            return featureCollection;
            //check account has access to EAN
        }

        featureCollection = await _placeStoreQuery.SearchByKeyword(placeKeywordSearchQuery);
        return featureCollection;
    }
}
