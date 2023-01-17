using CouchDBService;
using DFM.API;
using DFM.API.Repositories;
using DFM.Shared.Configurations;
using DFM.Shared.Extensions;
using DFM.Shared.Helper;
using DFM.Shared.Interfaces;
using DFM.Shared.Repository;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

string? CorsPolicy = "CorsPolicy";
string? XmlCommentsFileName = "DFM.API.xml";

//#region Change appsettings when in develop
//#if DEBUG
//builder.Configuration.AddJsonFile("appsettings.Development.json", false, true);
//#else
//builder.Configuration.AddJsonFile("appsettings.json", false, true);
//#endif
//#endregion

#region Custom log used serial log

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .WriteTo.Elasticsearch
    (
        new ElasticsearchSinkOptions
        (
            new Uri(context.Configuration["LogServer:Uri"])
        )
        {
            IndexFormat = $"{context.Configuration["AppName"]}-logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.Now:yyyy-MM}",
            AutoRegisterTemplate = true,
            NumberOfShards = 2,
            NumberOfReplicas = 1
        }
    )
    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
    .ReadFrom.Configuration(context.Configuration);
});
#endregion

#region Add Cors
builder.Services.AddCors(option =>
{
    option.AddPolicy(name: CorsPolicy,
        builder =>
        {
            builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});
#endregion


#region Add services to the container.
// Add HttpClientService
builder.Services.RegisterHttpClientService();
// Add CouchDB Service
builder.Services.AddSingleton<ICouchContext, CouchContext>();

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

// Redis Cache for IDistributedCache
//var redisOptions = ConfigurationOptions.Parse($"{redisConf.Server}:{redisConf.Port}, password={redisConf.Password}");
//redisOptions.User = redisConf.User;
//redisOptions.Password = redisConf.Password;
//redisOptions.Ssl = true;
builder.Services.AddStackExchangeRedisCache(options => // Register redis cache
{
    //options.ConfigurationOptions = redisOptions;
    options.Configuration = $"{redisConf.Server}:{redisConf.Port}, password={redisConf.Password}";
    options.InstanceName = redisConf.Instance;
});

builder.Services.AddSingleton<IRedisConnector, RedisConnector>();
builder.Services.AddSingleton<IDocumentSecurity, DocumentSecurity>();
builder.Services.AddSingleton<IDocumentTransaction, DocumentTransaction>();
builder.Services.AddSingleton<IDocumentType, DocumentType>();
builder.Services.AddSingleton<IDocumentUrgent, DocumentUrgent>();
builder.Services.AddSingleton<IEmployeeManager, EmployeeManager>();
builder.Services.AddSingleton<IFolderManager, FolderManager>();
builder.Services.AddSingleton<IOrganizationChart, OrganizationChart>();
builder.Services.AddSingleton<IRoleManager, RoleManager>();
builder.Services.AddSingleton<IUserManager, UserManager>();
builder.Services.AddSingleton<IMinioService, MinioService>();
builder.Services.AddHostedService<IndexCreationService>();
builder.Services.AddSingleton<IIdentityHelper, IdentityHelper>();
builder.Services.AddSingleton<IEmailHelper,  EmailHelper>();
builder.Services.AddSingleton<IAESHelper, AESHelper>();

#endregion
builder.Services.AddControllers();



#region Register swagger endpoint

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
// Add Swagger with API Versioning
builder.Services.AddApiVersioning(options =>
{
    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
    options.ReportApiVersions = true;
});
builder.Services.AddVersionedApiExplorer(options =>
{
    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
    // note: the specified format code will format the version as "'v'major[.minor][-status]"
    options.GroupNameFormat = "'v'VVV";
    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
    // can also be used to control the format of the API version in route templates
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    // add a custom operation filter which sets default values
    options.OperationFilter<SwaggerDefaultValues>();
    // integrate xml comments
    options.IncludeXmlComments(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), XmlCommentsFileName));
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{openId.Authority}/connect/authorize"),
                Scopes = new Dictionary<string, string> {
                                { openId.APIScope!, "Scope DFM API Gateway" }
                            }
            }
        }
    });
});
#endregion

#region Register OpenID
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
    .AddIdentityServerAuthentication(options =>
    {
        // base-address of your identityserver
        options.Authority = openId.Authority;
        options.RequireHttpsMetadata = false;
        // name of the API resource
        options.ApiName = openId.ApiName;
        options.ApiSecret = openId.ApiSecret;

        options.SupportedTokens = SupportedTokens.Reference;
    });
#endregion

var app = builder.Build();

app.UseSwagger();
app.AddForwardHeaders();
// Enable middleware to serve Swagger-UI (HTML, JS, CSS, etc.) by specifying the Swagger JSON endpoint(s).
var descriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwaggerUI(options =>
{
    // Build a swagger endpoint for each discovered API version
    foreach (var description in descriptionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    }
    options.OAuthClientId(openId.SwaggerUIClient);
    options.OAuthAppName(openId.ApiName);
});


app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
