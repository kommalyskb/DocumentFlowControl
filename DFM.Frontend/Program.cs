using CouchDBService;
using DFM.Frontend.Data;
using DFM.Shared.Configurations;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using Microsoft.JSInterop;
using MudBlazor.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.RegisterHttpClientService();

// Register configuration
var openId = builder.Configuration.GetSection(nameof(OpenIDConf)).Get<OpenIDConf>();
builder.Services.AddSingleton(openId);

var DBConfigConf = builder.Configuration.GetSection(nameof(DBConfig)).Get<DBConfig>();
builder.Services.AddSingleton(DBConfigConf);

var redisConf = builder.Configuration.GetSection(nameof(RedisConf)).Get<RedisConf>();
builder.Services.AddSingleton(redisConf);

var logConf = builder.Configuration.GetSection(nameof(LogServer)).Get<LogServer>();
builder.Services.AddSingleton(logConf);

var endpointConf = builder.Configuration.GetSection(nameof(ServiceEndpoint)).Get<ServiceEndpoint>();
builder.Services.AddSingleton(endpointConf);

var storageConf = builder.Configuration.GetSection(nameof(StorageConfiguration)).Get<StorageConfiguration>();
builder.Services.AddSingleton(storageConf);

// Redis Cache for IDistributedCache
var redisOptions = ConfigurationOptions.Parse($"{redisConf.Server}:{redisConf.Port}");
//redisOptions.User = redisConf.User;
//redisOptions.Password = redisConf.Password;
//redisOptions.Ssl = true;
builder.Services.AddStackExchangeRedisCache(options => // Register redis cache
{
    options.ConfigurationOptions = redisOptions;
    options.InstanceName = redisConf.Instance;
});

builder.Services.AddSingleton<IRedisConnector, RedisConnector>();
builder.Services.AddScoped<AccessTokenStorage>();
builder.Services.AddScoped<LocalStorageHelper>();
builder.Services.AddScoped<ICascadingService, CascadingService>();
builder.Services.AddSingleton<IMinioService, MinioService>();
//builder.Services.AddScoped<IJSRuntime, JSRuntime>();
builder.Services.AddMudServices();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
