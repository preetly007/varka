using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unica.Core.Contracts.Models;
using Varca.Domain.Models;

namespace Varca.Domain.Repositories;

public interface IProviderSubscribedStore
{
    Task<List<ClientProviderInfo>> GetManyAsync(string accountId, bool enabled = true);
    Task<List<ProviderPublishKey>> GetManyPublishKeyAsync(string accountId, bool enabled = true);
}
