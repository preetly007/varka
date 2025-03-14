using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Unica.Core.Contracts.Models;
using Varca.Domain;
using Varca.Domain.Models;
using Varca.Domain.Repositories;

namespace Varca.Persistence.Mongo;

public class MongoProviderSubscribedStore : IProviderSubscribedStore
{
    private readonly ILogger<MongoProviderSubscribedStore> _logger;
    public MongoProviderSubscribedStore(ILogger<MongoProviderSubscribedStore> logger) {
        _logger = logger;
    }
    public async Task<List<ClientProviderInfo>> GetManyAsync(string accountId, bool enabled = true)
    {
        var providers = new List<ClientProviderInfo>();
        var unicaConfigDb = VarcaApp.This.UnicaReadOnlyStore.GetDatabase(VarcaApp.This.Configs.UnicaDbReadOnly.UnicaConfigDb);
        var providerCollectionDb = unicaConfigDb.GetCollection<ClientProviderInfo>(VarcaApp.This.Configs.UnicaDbReadOnly.UnicaProvidersCollection);

        var accountFilter = Builders<ClientProviderInfo>.Filter.Eq(x => x.AccountId, accountId);
        var disabledFilter = Builders<ClientProviderInfo>.Filter.Eq(x => x.IsDisabled, !enabled);
        var filter = Builders<ClientProviderInfo>.Filter.And(accountFilter, disabledFilter);

        var selectFields = GetClientProviderInfoProjectedFields();                    
        var findOptions = new FindOptions<ClientProviderInfo> {
            Projection = selectFields
        };

        using (var clientProviderInfoCursor = await providerCollectionDb.FindAsync(filter, findOptions).ConfigureAwait(false)) {
            while (await clientProviderInfoCursor.MoveNextAsync().ConfigureAwait(false)) {
                providers.AddRange(clientProviderInfoCursor.Current);
            }
        }
        return providers;
    }

    private ProjectionDefinition<ClientProviderInfo, ClientProviderInfo> GetClientProviderInfoProjectedFields()
    {
        var builder = Builders<ClientProviderInfo>.Projection;
        var selectFields = builder.Include(x => x.AccountId)
                            .Include(x => x.CredentialsAccountId)
                            .Include(x => x.ApprovedChannelIds)
                            .Include(x => x.ApprovalStatus)
                            .Include(x => x.AvailableChannelIds)
                            .Include(x => x.ProviderFamily)
                            .Include(x => x.IsExclusiveDownloader)
                            .Include(x => x.DownloaderType)
                            .Include(x => x.IsPrivate)
                            .Include(x => x.MasterProviderKey)
                            .Include(x => x.IsDisabled);
        return selectFields;
    }

    public async Task<List<ProviderPublishKey>> GetManyPublishKeyAsync(string accountId, bool enabled = true)
    {
        //TODO: ApprovalStatus should not be null for valid providers
        var publishKeys = new List<ProviderPublishKey>();
        var providers = await GetManyAsync(accountId, enabled).ConfigureAwait(false);
        providers.ForEach(x => {
            if (x.ApprovalStatus == null || x.ApprovalStatus.Trim().Equals("enabled", StringComparison.InvariantCultureIgnoreCase)) 
                publishKeys.Add(new ProviderPublishKey(x.ProviderFamily, x.GetPublishKey(), x.AvailableChannelIds, x.ApprovedChannelIds));
        });
        return publishKeys;
    }
}
