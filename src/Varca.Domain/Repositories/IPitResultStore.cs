using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Varca.Domain.Models;

namespace Varca.Domain.Repositories;

public interface IPitResultStore
{
    Task<PitGeometryNew?> SaveGeometryAsync(PitGeometryNew pitNew);
    Task<PitPlaceNew?> SavePlaceAsync(PitPlaceNew pitNew);
    Task SaveMappingsAsync(string accountId, string pitId, GeoHotelMappingCollection? mappings);
    Task SaveContentAsync(string accountId, string pitId);
    Task<GeoHotelMappingCollection?> GetMappingsAsync(string accountId, string pitId);
    Task GetContentAsync(string accountId, string pitId);
    Task<PitInfo?> GetByIdAsync(string pitId, string accountId);
    Task SaveProcessStartAtAsync(string pitId, string accountId, DateTimeOffset processStartedAtUtc);
}