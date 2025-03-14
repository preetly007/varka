// using System;
// using System.Collections.Generic;
// using System.Linq;
// // using Elastic.Clients.Elasticsearch;
// // using Elastic.Clients.Elasticsearch.QueryDsl;
// // using Elastic.Transport;

// using Elasticsearch.Net;
// using Nest;
// using Varca.Domain.Models;
// using Varca.Domain.Repositories;

// namespace Varca.Persistence.ES;

// public class EsGeoHotelMappingStoreQuery : IGeoHotelMappingStoreQuery
// {
//     private readonly ElasticClient _elasticsearchClient;
//     private const string INDEX = "search-geo-hotelmappings-v2";
//     public EsGeoHotelMappingStoreQuery() {
//         var connectionPool = new  SingleNodeConnectionPool(new Uri("https://localhost:9200"));

//         var ConnectionSettings = new ConnectionSettings(connectionPool)
//             .CertificateFingerprint("bb8f570cb2c7d839d90107a4621f49da9d755432c7abcacc7a6839741fc40333")
//             .BasicAuthentication("elastic", "aA=SYK1gA5z*At+1uQ6y")
//             //.ApiKeyAuthentication("devtest", "NlZ4Ym9vNEJHNXFlUWwwaXNJT1g6MnB2UEpLQ2lRbWV5ekxEMWxrOF9ldw==")
//             //.ThrowExceptions(true)
//             .EnableApiVersioningHeader(true);

            
//         _elasticsearchClient = new ElasticClient(ConnectionSettings);



//         // var nodes = new Uri[] {
//         //     new("https://localhost:9200")
//         // };
        
//         // var pool = new StaticNodePool(nodes);

//         // var settings = new ElasticsearchClientSettings(pool)
//         //     .CertificateFingerprint("bb8f570cb2c7d839d90107a4621f49da9d755432c7abcacc7a6839741fc40333")
//         //     .Authentication(new ApiKey("NlZ4Ym9vNEJHNXFlUWwwaXNJT1g6MnB2UEpLQ2lRbWV5ekxEMWxrOF9ldw=="));

//         // _elasticsearchClient = new ElasticsearchClient(settings);
//     }

//     public async Task<GeoHotelMappingCollection> SearchByPointAsync(PointGeometryQuery pointGeometryQuery)
//     {
//         // // var geoLocation = GeoLocation.Coordinates([pointGeometryQuery.Geometry.Coordinates[1], 
//         // //                                     pointGeometryQuery.Geometry.Coordinates[0]]);

//         // var geoLocation = new GeoLocation(pointGeometryQuery.Geometry.Coordinates[1], 
//         //                                     pointGeometryQuery.Geometry.Coordinates[0]);

//         // List<string> publishKeys = [];
//         // pointGeometryQuery.Filter.EnabledProviderPublishKey.ForEach(x => publishKeys.Add(x.PublishKey));

//         // // var queryResponse = await _elasticsearchClient.SearchAsync<GeoHotelMapping>(s => s
//         // // .Index(INDEX)
//         // // .From(0)
//         // // .Size(10000)
//         // // .Query(q => q
//         // //     .Bool(b => b
//         // //         .Must(m => m
//         // //             .Terms(ts =>  ts
//         // //                 .Field(o => o.PublishKey)
//         // //                 .Terms(new TermsQueryField(pointGeometryQuery.Filter.EnabledProviderPublishKey.Select(x => FieldValue.String(x.PublishKey)).ToArray()))
//         // //             )
//         // //         )
//         // //         .Filter(f => f
//         // //             .GeoDistance(gd => gd
//         // //                 .Field(fd => fd.Location)
//         // //                 .Distance(pointGeometryQuery.Radius)
//         // //                 .Location(geoLocation)
//         // //                 .DistanceType(GeoDistanceType.Arc)
//         // //                 .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
//         // //             )  
//         // //         ) 
//         // //     )
//         // // )

//         // var queryResponse = await _elasticsearchClient.SearchAsync<GeoHotelMapping>(s => s
//         // .Index(INDEX)
//         // .From(0)
//         // .Size(10000)
//         // .Query(q => q
//         //     .Bool(b => b
//         //         .Must(m => m
//         //             .Terms(ts =>  ts
//         //                 .Field(o => o.PublishKey)
//         //                 .Terms(publishKeys)
//         //             )
//         //         )
//         //         .Filter(f => f
//         //             .GeoDistance(gd => gd
//         //                 .Field(fd => fd.Location)
//         //                 .Distance(pointGeometryQuery.Radius)
//         //                 .Location(geoLocation)
//         //                 .DistanceType(GeoDistanceType.Arc)
//         //                 .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
//         //             )  
//         //         ) 
//         //     )
//         // )
//         // // .Sort(sort => sort
//         // //         .GeoDistance(g => g
//         // //             .Field(f => f.Location)
//         // //             .Order(SortOrder.Ascending)
//         // //             .Unit(DistanceUnit.Meters)
//         // //             .Points(geoLocation)
//         // //             .DistanceType(GeoDistanceType.Arc)
//         // //             .IgnoreUnmapped()
//         // //         )
//         // //     )
//         // );

//         // var mappings = new GeoHotelMappingCollection();
//         // if (queryResponse.IsValid) {
//         //     mappings.Mappings = queryResponse.Documents.ToList();
//         // } 
//         // return mappings;



//         var geoLocation = new GeoLocation(pointGeometryQuery.Geometry.Coordinates[1], 
//                                             pointGeometryQuery.Geometry.Coordinates[0]);

//         List<string> publishKeys = [];
//         pointGeometryQuery.Filter.EnabledProviderPublishKey.ForEach(x => publishKeys.Add(x.PublishKey.ToLower()));

//         var queryResponse = await _elasticsearchClient.SearchAsync<GeoHotelMapping>(s => s
//         .Index(INDEX)
//         .From(0)
//         .Size(10000)
//         .Query(q => q
//             .Bool(b => b
//                 .Must(m => m
//                     .Terms(ts =>  ts
//                         .Field(o => o.PublishKey)
//                         .Terms(publishKeys)
//                     )
//                 )
//                 .Filter(f => f
//                     .GeoDistance(gd => gd
//                         .Field(fd => fd.GeoCode)
//                         .Distance(pointGeometryQuery.Radius)
//                         .Location(geoLocation)
//                         .DistanceType(GeoDistanceType.Arc)
//                         .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
//                     )  
//                 ) 
//             )
//         )
//         // .Sort(sort => sort
//         //         .GeoDistance(g => g
//         //             .Field(f => f.Location)
//         //             .Order(SortOrder.Ascending)
//         //             .Unit(DistanceUnit.Meters)
//         //             .Points(geoLocation)
//         //             .DistanceType(GeoDistanceType.Arc)
//         //             .IgnoreUnmapped()
//         //         )
//         //     )
//         );

//         var mappings = new GeoHotelMappingCollection();
//         if (queryResponse.IsValid) {
//             mappings.Mappings = queryResponse.Documents.ToList();
//         } 
//         return mappings;

//     }

//     public async Task<GeoHotelMappingCollection> SearchByPolygonAsync(PolygonGeometryQuery polygonQuery)
//     {
//         List<string> publishKeys = [];
//         polygonQuery.Filter.EnabledProviderPublishKey.ForEach(x => publishKeys.Add(x.PublishKey.ToLower()));

//         var geoCoordinates = new List<GeoCoordinate>();
//         polygonQuery.Geometry.Coordinates.First().ForEach(x => {
//             geoCoordinates.Add(new GeoCoordinate(x[1], x[0]));
//         });


//         var queryResponse = await _elasticsearchClient.SearchAsync<GeoHotelMapping>(s => s
//             .Index(INDEX)
//             .From(0)
//             .Size(10000)
//             .Query(q => q
//                 .Bool(b => b
//                     .Must(m => m
//                         .Terms(ts =>  ts
//                             .Field(o => o.PublishKey)
//                             .Terms(publishKeys)
//                             //.Terms(new TermsQueryField(polygonQuery.Filter.EnabledProviderPublishKey.Select(x => FieldValue.String(x.PublishKey)).ToArray()))
//                         )
//                     )
                    
//                     // .Filter(f => f
//                     //     .GeoShape(gs => gs
//                     //         .Field(p => p.Location)
//                     //         .Shape(sp => sp
//                     //             .Polygon(geoCoordinates)
//                     //         )
//                     //         .Relation(GeoShapeRelation.Within)
//                     //     )
//                     // ) 
//                 )
//             )
//         );
        
//         var mappings = new GeoHotelMappingCollection();
//         if (queryResponse.IsValid) {
//             mappings.Mappings = queryResponse.Documents.ToList();
//         } 
//         return mappings;
//     }

//     public async Task<GeoHotelMappingCollection> SearchByPolygonAsync(SearchMappingsGeometry searchPolygon)
//     {
//         throw new NotImplementedException();
//     }

//     public async Task<GeoHotelMappingCollection> SearchByPointAsync(SearchMappingsGeometry searchPoint)
//     {
//         List<string> publishKeys = [];

//         searchPoint.Filters.EnabledProviders.ForEach(x => {
//             x.ChannelIds
//         }) 
        
        
//         //.EnabledProviderPublishKey.ForEach(x => publishKeys.Add(x.PublishKey.ToLower()));

//     }
// }
