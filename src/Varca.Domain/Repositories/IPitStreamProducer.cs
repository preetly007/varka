using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Varca.Domain.Models;

namespace Varca.Domain.Repositories;

public interface IPitStreamProducer
{
    Task<PitGeometryNew?> SendGeometryAsync(PitGeometryNew pitNew);
    Task<PitPlaceNew?> SendPlaceAsync(PitPlaceNew pitNew);
}
