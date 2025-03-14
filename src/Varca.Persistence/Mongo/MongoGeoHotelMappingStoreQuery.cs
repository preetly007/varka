using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using GeoJSON.Text.Geometry;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Varca.Domain;
using Varca.Domain.Models;
using Varca.Domain.Repositories;

namespace Varca.Persistence.Mongo;

public class MongoGeoHotelMappingStoreQuery : IGeoHotelMappingStoreQuery
{
    private readonly ILogger<MongoGeoHotelMappingStoreQuery> _logger;
    
    public MongoGeoHotelMappingStoreQuery(ILogger<MongoGeoHotelMappingStoreQuery> logger) {
        _logger = logger;
    }
    public async Task<GeoHotelMappingCollection?> SearchByPointAsync(SearchMappingsGeometry searchPoint)
    {
        var varcaDb = VarcaApp.This.GeoHotelMappingsStore.GetDatabase(VarcaApp.This.Configs.GeoStore.Db);
        var varcaGeoHotelMappings = varcaDb.GetCollection<GeoHotelMapping>(VarcaApp.This.Configs.GeoStore.GhmsCollection);

        var queryBuilder = Builders<GeoHotelMapping>.Filter;

        var acchessKeysFilter = queryBuilder.AnyIn(x => x.AccessKeys, searchPoint.Filters.AccessKeys);

        var searchQueryPoint = (Point)searchPoint.Geometry;
        var pointGeometryQuery = GeoJson.Point(GeoJson.Position(searchQueryPoint.Coordinates.Longitude, searchQueryPoint.Coordinates.Latitude));
        var locationGeometryFilter = queryBuilder.NearSphere(x => x.GeoLocation, pointGeometryQuery, searchPoint.Radius * 1000);

        var andFilters = queryBuilder.And(acchessKeysFilter, locationGeometryFilter);

        if (searchPoint.Filters.PropertyTypes != null && searchPoint.Filters.PropertyTypes.Count > 0) {
            var propertyTypeFilter = queryBuilder.In(x => x.PropertyType, searchPoint.Filters.PropertyTypes);
            andFilters &= propertyTypeFilter;
        }

        if (searchPoint.Filters.Ratings != null && searchPoint.Filters.Ratings.Count > 0) {
            var ratingsFilter = queryBuilder.In(x => x.Rating, searchPoint.Filters.Ratings);
            andFilters &= ratingsFilter;
        }

        if (searchPoint.Filters.ChainCodes != null && searchPoint.Filters.ChainCodes.Count > 0) {
            var chainCodesFilter = queryBuilder.In(x => x.ChainCode, searchPoint.Filters.ChainCodes);
            andFilters &= chainCodesFilter;
        }

        var selectFields = GetMappingProjectedFields();                    
        var findOptions = new FindOptions<GeoHotelMapping> {
            Projection = selectFields,
            Hint = VarcaApp.This.Configs.GeoStore.GhmsIdx
        };

        LogQuery(andFilters);

        var geoHotelMappings = new GeoHotelMappingCollection {
            Mappings = new List<GeoHotelMapping>()
        };

        using (var clientProviderInfoCursor = await varcaGeoHotelMappings.FindAsync(andFilters, findOptions).ConfigureAwait(false)) {
            while (await clientProviderInfoCursor.MoveNextAsync().ConfigureAwait(false)) {
                geoHotelMappings.Mappings.AddRange(clientProviderInfoCursor.Current);
            }
        }

        return geoHotelMappings;
    }

    private static ProjectionDefinition<GeoHotelMapping> GetMappingProjectedFields() 
    {
        var builder = Builders<GeoHotelMapping>.Projection;
        var selectFields = builder.Include(x => x.ChainCode)
                            .Include(x => x.ChannelIds)
                            .Include(x => x.GeoLocation)
                            .Include(x => x.PropertyType)
                            .Include(x => x.ProviderFamily)
                            .Include(x => x.ProviderHotelId)
                            .Include(x => x.ProviderLocationCode)
                            .Include(x => x.PublishKey)
                            .Include(x => x.Rating)
                            .Include(x => x.UnicaId);
        return selectFields;

    }
    private static BsonDocument GetProjectedFields()
    {
        return new BsonDocument {
            { "ChannelIds", new BsonDocument("$filter", new BsonDocument
                {
                    { "input", "$ChannelIds" },
                    { "as", "n" },
                    { "cond", new BsonDocument("$ne", new BsonArray
                        {
                            "$$n",
                            "_*"
                        }) }
                }) },
            { "PropertyType", 1 },
            { "PublishKey", 1 },
            { "ProviderFamily", 1 },
            { "UnicaId", 1 },
            { "ProviderHotelId", 1 },
            { "ProviderLocationCode", 1 },
            { "ChainCode", 1 },
            { "Rating", 1 },
            { "GeoLocation", 1}
        };
    }

    private static void LogQuery(FilterDefinition<GeoHotelMapping> filter)
    {
        var serializerRegistry = BsonSerializer.SerializerRegistry;
        var documentSerializer = serializerRegistry.GetSerializer<GeoHotelMapping>();
        var rendered = filter.Render(documentSerializer, serializerRegistry);
        var query = rendered.ToJson(new JsonWriterSettings { Indent = true });
        Console.WriteLine(query);
    }
    public async Task<GeoHotelMappingCollection?> SearchByPolygonAsync(SearchMappingsGeometry searchPolygon)
    {
        var varcaDb = VarcaApp.This.GeoHotelMappingsStore.GetDatabase(VarcaApp.This.Configs.GeoStore.Db);
        var varcaGeoHotelMappings = varcaDb.GetCollection<GeoHotelMapping>(VarcaApp.This.Configs.GeoStore.GhmsCollection);

        var searchPolygonGeometry = (Polygon)searchPolygon.Geometry;
        var searchPolygonGeometryCoordinates = searchPolygonGeometry.Coordinates[0];
        
        var polygonCoordinatesQuery = new GeoJson2DCoordinates[searchPolygonGeometryCoordinates.Coordinates.Count];
        for(int index = 0; index < searchPolygonGeometryCoordinates.Coordinates.Count; index++) {
            polygonCoordinatesQuery[index] = GeoJson.Position(searchPolygonGeometryCoordinates.Coordinates[index].Longitude, searchPolygonGeometryCoordinates.Coordinates[index].Latitude);
        }
        var polygonGeometryQuery = GeoJson.Polygon(polygonCoordinatesQuery);


        var queryBuilder = Builders<GeoHotelMapping>.Filter;
        var polygonFilter = queryBuilder.GeoWithin(x => x.GeoLocation, polygonGeometryQuery);

        var acchessKeysFilter = queryBuilder.AnyIn(x => x.AccessKeys, searchPolygon.Filters.AccessKeys);

        var andFilters = queryBuilder.And(acchessKeysFilter, polygonFilter);

        if (searchPolygon.Filters.PropertyTypes != null && searchPolygon.Filters.PropertyTypes.Count > 0) {
            var propertyTypeFilter = queryBuilder.In(x => x.PropertyType, searchPolygon.Filters.PropertyTypes);
            andFilters &= propertyTypeFilter;
        }

        if (searchPolygon.Filters.Ratings != null && searchPolygon.Filters.Ratings.Count > 0) {
            var ratingsFilter = queryBuilder.In(x => x.Rating, searchPolygon.Filters.Ratings);
            andFilters &= ratingsFilter;
        }

        if (searchPolygon.Filters.ChainCodes != null && searchPolygon.Filters.ChainCodes.Count > 0) {
            var chainCodesFilter = queryBuilder.In(x => x.ChainCode, searchPolygon.Filters.ChainCodes);
            andFilters &= chainCodesFilter;
        }

        var selectFields = GetMappingProjectedFields();
        var findOptions = new FindOptions<GeoHotelMapping> {
            Projection = selectFields,
            Hint = VarcaApp.This.Configs.GeoStore.GhmsIdx
        };

        LogQuery(andFilters);
        var geoHotelMappings = new GeoHotelMappingCollection{
            Mappings = new List<GeoHotelMapping>()
        };
        
        using (var clientProviderInfoCursor = await varcaGeoHotelMappings.FindAsync(andFilters, findOptions).ConfigureAwait(false)) {
            while (await clientProviderInfoCursor.MoveNextAsync().ConfigureAwait(false)) {
                geoHotelMappings.Mappings.AddRange(clientProviderInfoCursor.Current);
            }
        }

        return geoHotelMappings;
    }

    public async Task<GeoHotelMappingCollection?> SearchByMultiPolygonAsync(SearchMappingsGeometry searchPolygon)
    {
        var varcaDb = VarcaApp.This.GeoHotelMappingsStore.GetDatabase(VarcaApp.This.Configs.GeoStore.Db);
        var varcaGeoHotelMappings = varcaDb.GetCollection<GeoHotelMapping>(VarcaApp.This.Configs.GeoStore.GhmsCollection);

        var searchMultiPolygonGeometry = (MultiPolygon)searchPolygon.Geometry;
        
        int count = searchMultiPolygonGeometry.Coordinates.Count;
        var multiPolygonCoordinates = new  GeoJsonPolygonCoordinates<GeoJson2DCoordinates>[count];

        for( int indexMultipolygon = 0; indexMultipolygon < count; indexMultipolygon++)
        {
            var polygon = searchMultiPolygonGeometry.Coordinates[indexMultipolygon];
            var polygonFirst = polygon.Coordinates[0];
            var polygonCoordinatesQuery = new GeoJson2DCoordinates[polygonFirst.Coordinates.Count];
            for(int indexCoordinates = 0; indexCoordinates < polygonFirst.Coordinates.Count; indexCoordinates++) {
                polygonCoordinatesQuery[indexCoordinates] = GeoJson.Position(polygonFirst.Coordinates[indexCoordinates].Longitude, polygonFirst.Coordinates[indexCoordinates].Latitude);
            }
            multiPolygonCoordinates[indexMultipolygon] = GeoJson.PolygonCoordinates(polygonCoordinatesQuery);            
        }

        var multiPolygon = GeoJson.MultiPolygon(multiPolygonCoordinates);

        var queryBuilder = Builders<GeoHotelMapping>.Filter;

        var multiPolygonFilter = queryBuilder.GeoWithin(x => x.GeoLocation, multiPolygon);
        var acchessKeysFilter = queryBuilder.AnyIn(x => x.AccessKeys, searchPolygon.Filters.AccessKeys);

        var andFilters = queryBuilder.And(acchessKeysFilter, multiPolygonFilter);
        
        if (searchPolygon.Filters.PropertyTypes != null && searchPolygon.Filters.PropertyTypes.Count > 0) {
            var propertyTypeFilter = queryBuilder.In(x => x.PropertyType, searchPolygon.Filters.PropertyTypes);
            andFilters &= propertyTypeFilter;
        }

        if (searchPolygon.Filters.Ratings != null && searchPolygon.Filters.Ratings.Count > 0) {
            var ratingsFilter = queryBuilder.In(x => x.Rating, searchPolygon.Filters.Ratings);
            andFilters &= ratingsFilter;
        }

        if (searchPolygon.Filters.ChainCodes != null && searchPolygon.Filters.ChainCodes.Count > 0) {
            var chainCodesFilter = queryBuilder.In(x => x.ChainCode, searchPolygon.Filters.ChainCodes);
            andFilters &= chainCodesFilter;
        }

        var selectFields = GetMappingProjectedFields();
        var findOptions = new FindOptions<GeoHotelMapping> {
            Projection = selectFields,
            Hint = VarcaApp.This.Configs.GeoStore.GhmsIdx
        };

        LogQuery(andFilters);
        var geoHotelMappings = new GeoHotelMappingCollection{
            Mappings = new List<GeoHotelMapping>()
        };
        
        using (var clientProviderInfoCursor = await varcaGeoHotelMappings.FindAsync(andFilters, findOptions).ConfigureAwait(false)) {
            while (await clientProviderInfoCursor.MoveNextAsync().ConfigureAwait(false)) {
                geoHotelMappings.Mappings.AddRange(clientProviderInfoCursor.Current);
            }
        }

        return geoHotelMappings;
    }
}
