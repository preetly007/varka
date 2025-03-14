using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Varca.Domain.Models;

namespace Varca.Domain.Services;

public interface IPitService
{
    Task<PitGeometryNew?> CreateByPointAsync(PitGeometryNew pitNew);
    Task<PitGeometryNew?> CreateByPolygonAsync(PitGeometryNew pitNew);
    Task<PitGeometryNew?> CreateByMultiPolygonAsync(PitGeometryNew pitNew);
    Task<PitPlaceNew?> CreateByPlaceAsync(PitPlaceNew pitNew);
    Task<PitInfo?> GetByIdAsync(string pitId, string accountId);
}
