using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoJSON.Text.Feature;
using Varca.Domain.Models;

namespace Varca.Domain.Repositories;

public interface IPlaceStoreQuery
{
    Task<FeatureCollection?> SearchByKeyword(PlaceKeywordSearchQuery placeKeywordSearchQuery);
    Task<Feature?> GetByPlaceId(PlaceIdQuery placeIdQuery);
}
