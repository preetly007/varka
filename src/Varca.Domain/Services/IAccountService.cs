using System.Linq;
using System.Threading.Tasks;
using Unica.Core.Contracts.Models;
using Varca.Domain.Models;

namespace Varca.Domain.Services;

public interface IAccountService
{
    Task<SearchMappingsGeometry> BuildAccessKeysFilterAsync(SearchMappingsGeometry searchMappingsGeometry);
    Task<PitPlaceNew> BuildAccessKeysFilterAsync(PitPlaceNew pitNew);
    Task<PitGeometryNew> BuildAccessKeysFilterAsync(PitGeometryNew pitNew);
    Task<SearchMappingsPlace> BuildAccessKeysFilterAsync(SearchMappingsPlace searchPlace);
}