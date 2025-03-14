using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using GeoJSON.Text.Feature;
using Microsoft.Extensions.Logging;
using Varca.Domain;
using Varca.Domain.Models;
using Varca.Domain.Repositories;

namespace Varca.Persistence.ES;

public class EsVervotechPlaceStoreQuery : IPlaceStoreQuery
{
    private readonly ILogger<EsVervotechPlaceStoreQuery> _logger;
    private static readonly string[] fields = ["properties.name", "properties.displayName", "properties.oldNames", "properties.alternateNames"];

    public EsVervotechPlaceStoreQuery(ILogger<EsVervotechPlaceStoreQuery> logger) {
        _logger = logger;
    }

    public async Task<Feature?> GetByPlaceId(PlaceIdQuery placeIdQuery)
    {
        var getResponse = await VarcaApp.This.PlaceStore.GetAsync<Feature>(VarcaApp.This.Configs.PlaceStore.PlaceVervotechIdx, placeIdQuery.PlaceId);
        Feature? feature = default;
        if (getResponse.IsValidResponse) {
            feature = getResponse.Source;
        }
        return feature;
    }

    public async Task<FeatureCollection?> SearchByKeyword(PlaceKeywordSearchQuery placeKeywordSearchQuery)
    {
        var searchResponse = await VarcaApp.This.PlaceStore.SearchAsync<Feature>(s => s
            .Index(VarcaApp.This.Configs.PlaceStore.PlaceVervotechIdx)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(placeKeywordSearchQuery.Keyword)
                    .Type(TextQueryType.BoolPrefix)
                    .Operator(Operator.Or)
                    .Fields(fields)
                )
            )
            .From(0)
            .Size(placeKeywordSearchQuery.Limit)
        );

        FeatureCollection? featureCollection = default;
        if (searchResponse.IsValidResponse) {
            featureCollection = new FeatureCollection(searchResponse.Documents.ToList());
        }
        return featureCollection;
    }
}
