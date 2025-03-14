using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoJSON.Text.Feature;
using MongoDB.Driver;
using Varca.Domain.Models;

namespace Varca.Domain.Services;

public interface IPlaceSearchService
{
    Task<FeatureCollection?> SearchByKeywordAsync(PlaceKeywordSearchQuery placeKeywordSearchQuery);
    Task<Feature?> GetByPlaceIdAsync(PlaceIdQuery placeIdQuery);
}
