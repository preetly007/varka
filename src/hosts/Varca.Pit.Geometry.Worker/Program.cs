using AutoMapper;
using Varca.Caching.Redis;
using Varca.Domain;
using Varca.Domain.Repositories;
using Varca.Domain.Services;
using Varca.Persistence.ES;
using Varca.Persistence.Mongo;
using Varca.Persistence.Redis;
using Varca.Pit.Geometry.Worker;
using Varca.Pit.Geometry.Worker.Redis;


var builder = Host.CreateApplicationBuilder(args);

var varcaUberConfig = Environment.GetEnvironmentVariable("VARCA_UBER_CONFIG");
if (string.IsNullOrEmpty(varcaUberConfig)) {
    if (args.Length == 0) {
        Environment.Exit(-1);
    }
    varcaUberConfig = args[0].Trim();
}

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VARCA_PIT_GEOMETRY_CONSUMER_ID"))) {
    if (args.Length == 0) {
        Environment.Exit(-2);
    }
}


var varcaApp = new VarcaApp(varcaUberConfig);
builder.Services.AddSingleton<VarcaApp>(varcaApp);

builder.Services.AddHostedService<RedisPitGeometryWorker>();

builder.Services.AddLogging(logging =>
    logging.AddSimpleConsole(options => {
        options.SingleLine = true;
        options.TimestampFormat = "yyyy-MM-dd HH:mm:ss.fff ";
        options.UseUtcTimestamp = true;
    })
);

var autoMapperConfig = new MapperConfiguration(mc => {
    mc.AddProfile(new AutoMappingProfile());
});

builder.Services.AddSingleton(autoMapperConfig.CreateMapper());

builder.Services.AddScoped<IPitService, PitService>();
builder.Services.AddScoped<IPitStreamProducer, RedisPitStreamProducer>();
builder.Services.AddScoped<IGeoHotelMappingService, GeoHotelMappingService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IGeoHotelMappingStoreQuery, MongoGeoHotelMappingStoreQuery>();
//builder.Services.AddScoped<IGeoHotelMappingStoreQuery, EsGeoHotelMappingStoreQuery>();
builder.Services.AddScoped<IProviderSubscribedStore, MongoProviderSubscribedStore>();
builder.Services.AddKeyedScoped<IProviderSubscribedStore, RedisProviderSubscribedStore>(CacheProvider.Cacheable);
builder.Services.AddScoped<IPitResultStore, RedisPitResultStore>();
builder.Services.AddScoped<IPlaceSearchService, PlaceSearchService>();
builder.Services.AddScoped<IPlaceStoreQuery, EsVervotechPlaceStoreQuery>();  
builder.Services.AddTransient<CacheProvider>();


var host = builder.Build();
host.Run();
