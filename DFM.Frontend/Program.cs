using CouchDBService;
using DFM.Frontend.Data;
using DFM.Shared.Configurations;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using Microsoft.JSInterop;
using MudBlazor.Services;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.AspNetCore.ResponseCompression;
using DFM.Frontend.Hubs;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using DFM.Frontend.AuthHelper;

var builder = WebApplication.CreateBuilder(args);
#region Custom log used serial log

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    //.WriteTo.Elasticsearch
    //(
    //    new ElasticsearchSinkOptions
    //    (
    //        new Uri(context.Configuration["LogServer:Uri"])
    //    )
    //    {
    //        IndexFormat = $"{context.Configuration["AppName"]}-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.Now:yyyy-MM}",
    //        AutoRegisterTemplate = true,
    //        NumberOfShards = 2,
    //        NumberOfReplicas = 1
    //    }
    //)
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
    .ReadFrom.Configuration(context.Configuration);
});
#endregion
//#region Change appsettings when in develop
//#if DEBUG
//builder.Configuration.AddJsonFile("appsettings.Development.json", false, true);
//#else
//builder.Configuration.AddJsonFile("appsettings.json", false, true);
//#endif
//#endregion
// Add services to the container.
builder.Services.RegisterHttpClientService();

// Register configuration
var openId = builder.Configuration.GetSection(nameof(OpenIDConf)).Get<OpenIDConf>();
builder.Services.AddSingleton(openId);
Console.WriteLine($"------------OpenID Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(openId));
Console.WriteLine($"-------------------------------------------");

var DBConfigConf = builder.Configuration.GetSection(nameof(DBConfig)).Get<DBConfig>();
builder.Services.AddSingleton(DBConfigConf);
Console.WriteLine($"----------Database Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(DBConfigConf));
Console.WriteLine($"-------------------------------------------");

var redisConf = builder.Configuration.GetSection(nameof(RedisConf)).Get<RedisConf>();
builder.Services.AddSingleton(redisConf);
Console.WriteLine($"-------------Redis Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(redisConf));
Console.WriteLine($"-------------------------------------------");

var logConf = builder.Configuration.GetSection(nameof(LogServer)).Get<LogServer>();
builder.Services.AddSingleton(logConf);
Console.WriteLine($"------------Log Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(logConf));
Console.WriteLine($"-------------------------------------------");

var endpointConf = builder.Configuration.GetSection(nameof(ServiceEndpoint)).Get<ServiceEndpoint>();
builder.Services.AddSingleton(endpointConf);
Console.WriteLine($"----------Endpoint Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(endpointConf));
Console.WriteLine($"-------------------------------------------");

var storageConf = builder.Configuration.GetSection(nameof(StorageConfiguration)).Get<StorageConfiguration>();
builder.Services.AddSingleton(storageConf);
Console.WriteLine($"-----------Storage Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(storageConf));
Console.WriteLine($"-------------------------------------------");

var smtpConf = builder.Configuration.GetSection(nameof(SMTPConf)).Get<SMTPConf>();
builder.Services.AddSingleton(smtpConf);
Console.WriteLine($"-----------SMTP Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(smtpConf));
Console.WriteLine($"-------------------------------------------");

var aesConf = builder.Configuration.GetSection(nameof(AESConfig)).Get<AESConfig>();
builder.Services.AddSingleton(aesConf);
Console.WriteLine($"-----------AES Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(aesConf));
Console.WriteLine($"-------------------------------------------");

var envConf = builder.Configuration.GetSection(nameof(EnvConf)).Get<EnvConf>();
builder.Services.AddSingleton(envConf);
Console.WriteLine($"-----------EnvConf Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(envConf));
Console.WriteLine($"-------------------------------------------");

var frontendSts = builder.Configuration.GetSection(nameof(FrontendIdentity)).Get<FrontendIdentity>();
builder.Services.AddSingleton(frontendSts);
Console.WriteLine($"-----------Frontend OpenID Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(frontendSts));
Console.WriteLine($"-------------------------------------------");

var sendgrid = builder.Configuration.GetSection(nameof(SendGridConf)).Get<SendGridConf>();
builder.Services.AddSingleton(sendgrid);
Console.WriteLine($"-----------Sendgrid Configurations----------");
Console.WriteLine(JsonSerializer.Serialize(sendgrid));
Console.WriteLine($"-------------------------------------------");

// Redis Cache for IDistributedCache
//var redisOptions = ConfigurationOptions.Parse($"{redisConf.Server}:{redisConf.Port}");
//redisOptions.User = redisConf.User;
//redisOptions.Password = redisConf.Password;
//redisOptions.Ssl = true;
//builder.Services.AddStackExchangeRedisCache(options => // Register redis cache
//{
//    //options.ConfigurationOptions = redisOptions;
//    options.Configuration = $"{redisConf.Server}:{redisConf.Port}, password={redisConf.Password}";
//    options.InstanceName = redisConf.Instance;
//});

builder.Services.AddSingleton<IRedisConnector, RedisConnector>();

builder.Services.AddScoped<AccessTokenStorage>();
builder.Services.AddScoped<LocalStorageHelper>();
builder.Services.AddScoped<TokenState>();

builder.Services.AddScoped<ICascadingService, CascadingService>();
builder.Services.AddSingleton<IMinioService, MinioService>();
builder.Services.AddSingleton<IAESHelper,  AESHelper>();
//builder.Services.AddScoped<IJSRuntime, JSRuntime>();
builder.Services.AddMudServices();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
    options.DisconnectedCircuitMaxRetained = 100;
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    options.MaxBufferedUnacknowledgedRenderBatches = 10;
})
            .AddHubOptions(options =>
            {
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.EnableDetailedErrors = true;
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.MaximumParallelInvocationsPerClient = 1;
                options.MaximumReceiveMessageSize = 86 * 1024;
                options.StreamBufferCapacity = 10;
            });
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

//register health check
builder.Services.AddHealthChecks();
//regsiter websocket send helper
builder.Services.AddSingleton<ISendSocketHelper, SendSocketHelper>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
// Use response compression
app.UseResponseCompression();
//use health check
app.MapHealthChecks("/healthcheck");

// Forward header
app.AddForwardHeaders();

app.UseStaticFiles();
// Use cookie policies
app.UseCookiePolicy();
app.UseSession();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<NotifyHub>("/notifyhub");
app.MapFallbackToPage("/_Host");

app.Run();
