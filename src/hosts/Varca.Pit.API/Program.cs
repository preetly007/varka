using System.Net;
using AutoMapper;
using Varca.Caching.Redis;
using Varca.Domain;
using Varca.Domain.Repositories;
using Varca.Domain.Services;
using Varca.Persistence.ES;
using Varca.Persistence.Mongo;
using Varca.Persistence.Redis;
using Varca.Pit.API;

var builder = WebApplication.CreateBuilder(args);

var varcaUberConfig = Environment.GetEnvironmentVariable("VARCA_UBER_CONFIG");
if (string.IsNullOrEmpty(varcaUberConfig)) {
    if (args.Length == 0) {
        Environment.Exit(-1);
    }
    varcaUberConfig = args[0].Trim();
}

var varcaApp = new VarcaApp(varcaUberConfig);
builder.Services.AddSingleton<VarcaApp>(varcaApp);

builder.WebHost.ConfigureKestrel((context, serverOptions) => {
    serverOptions.Listen(IPAddress.Parse(varcaApp.Configs.VarcaServices.Pit.Address) , varcaApp.Configs.VarcaServices.Pit.Port);
});
builder.WebHost.UseKestrel();
builder.WebHost.UseContentRoot(Directory.GetCurrentDirectory());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
builder.Services.AddScoped<IGeoHotelMappingStoreQuery, MongoGeoHotelMappingStoreQuery>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IProviderSubscribedStore, MongoProviderSubscribedStore>();
builder.Services.AddKeyedScoped<IProviderSubscribedStore, RedisProviderSubscribedStore>(CacheProvider.Cacheable);
builder.Services.AddScoped<IPitResultStore, RedisPitResultStore>();
builder.Services.AddScoped<IPlaceSearchService, PlaceSearchService>();
builder.Services.AddScoped<IPlaceStoreQuery, EsVervotechPlaceStoreQuery>();  
builder.Services.AddTransient<CacheProvider>();

var app = builder.Build();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();


app.Run();

