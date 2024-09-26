
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Swashbuckle.AspNetCore.SwaggerGen;
using TNO.API.BackgroundWorkItem;
using TNO.API.Config;
using TNO.API.Helpers;
using TNO.API.Keycloak;
using TNO.API.Middleware;
using TNO.API.SignalR;
using TNO.Ches;
using TNO.Core.Converters;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.DAL;
using TNO.Kafka;
using TNO.Keycloak;
using TNO.TemplateEngine;

DotNetEnv.Env.Load();
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var builder = WebApplication.CreateBuilder(args);
using var factory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = factory.CreateLogger<Program>();

builder
    .Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("connectionstrings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"connectionstrings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);
var config = builder.Configuration;
var env = builder.Environment;

// Add services to the container.
var jsonSerializerOptions = config.GetSerializerOptions();
builder.Services.AddSerializerOptions(config);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ClaimsPrincipal>(s => s.GetService<IHttpContextAccessor>()?.HttpContext?.User ?? new ClaimsPrincipal());
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader = true;
})
  .AddJsonOptions(options =>
  {
      options.JsonSerializerOptions.DefaultIgnoreCondition = jsonSerializerOptions.DefaultIgnoreCondition;
      options.JsonSerializerOptions.PropertyNameCaseInsensitive = jsonSerializerOptions.PropertyNameCaseInsensitive;
      options.JsonSerializerOptions.PropertyNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy;
      options.JsonSerializerOptions.WriteIndented = jsonSerializerOptions.WriteIndented;
      options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
      //   options.JsonSerializerOptions.Converters.Add(new Int32ToStringJsonConverter());
  });

builder.Services.AddOptions<KestrelServerOptions>().Bind(config.GetSection("Kestrel"));
builder.Services.AddOptions<FormOptions>().Bind(config.GetSection("Form"));
builder.Services.AddOptions<KafkaOptions>().Bind(config.GetSection("Kafka"));
builder.Services.AddOptions<SignalROptions>().Bind(config.GetSection("SignalR"));
var signalROptions = new SignalROptions();
config.GetSection("SignalR").Bind(signalROptions);

// The following dependencies provide dynamic authorization based on keycloak client roles.
builder.Services.AddOptions<TNO.API.Config.KeycloakOptions>().Bind(config.GetSection("Keycloak"));
builder.Services.AddSingleton<IAuthorizationHandler, KeycloakClientRoleHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, ClientRoleAuthorizationPolicyProvider>();
builder.Services.AddAuthorization(options =>
    {
        // options.AddPolicy("administrator", policy => policy.Requirements.Add(new KeycloakClientRoleRequirement("administrator")));
    });

IdentityModelEventSource.ShowPII = true;
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var section = config.GetSection("Keycloak");
        options.RequireHttpsMetadata = !env.IsDevelopment();
        options.Authority = section.GetValue<string>("Authority");
        options.Audience = section.GetValue<string>("Audience");
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            // ValidIssuer = section.GetValue<string>("Issuer"),
            ValidIssuers = section.GetValue<string>("Issuer")?.Split(",") ?? Array.Empty<string>(),
            ValidateIssuer = section.GetValue<bool>("ValidateIssuer"),
            // ValidAudience = section.GetValue<string>("Audience"),
            ValidAudiences = section.GetValue<string>("Audience")?.Split(",") ?? Array.Empty<string>(),
            ValidateAudience = section.GetValue<bool>("ValidateAudience"),
            ValidateLifetime = true
        };
        var secret = config["Keycloak:Secret"];
        if (!String.IsNullOrWhiteSpace(secret))
        {
            var key = Encoding.ASCII.GetBytes(secret);
            if (key.Length > 0) options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(key);
        }
        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments(signalROptions.HubPath))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                context.NoResult();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                throw new AuthenticationException("Failed to authenticate", context.Exception);
            },
            OnForbidden = context =>
            {
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddKafkaHubBackPlane(config);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, TNO.API.Config.Swagger.ConfigureSwaggerOptions>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    options.EnableAnnotations(false, true);
    options.CustomSchemaIds(o => o.FullName);
    options.OperationFilter<TNO.API.Config.Swagger.SwaggerDefaultValues>();
    options.DocumentFilter<TNO.API.Config.Swagger.SwaggerDocumentFilter>();
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Description = "Please enter into field the word 'Bearer' following by space and JWT",
        Type = SecuritySchemeType.ApiKey
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,

            },
            new List<string>()
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services
    .Configure<ApiOptions>(config.GetSection("API"))
    .AddScoped<IConnectionHelper, ConnectionHelper>()
    .AddScoped<INotificationHelper, NotificationHelper>()
    .AddScoped<IReportHelper, ReportHelper>()
    .AddScoped<IWorkOrderHelper, WorkOrderHelper>()
    .AddScoped<ITopicScoreHelper, TopicScoreHelper>()
    .AddScoped<IImpersonationHelper, ImpersonationHelper>()
    .AddChesService(config.GetSection("CHES"))
    .AddTNOServices(config, env)
    .AddS3Config(config.GetSection("S3"))
    .AddTemplateEngine(config)
    .AddKafkaMessenger(config)
    .AddHttpClient()
    .AddScoped<JwtSecurityTokenHandler>()
    .AddTransient<IHttpRequestClient, HttpRequestClient>()
    .AddScoped<IKeycloakHelper, KeycloakHelper>()
    .AddScoped<IOpenIdConnectRequestClient, OpenIdConnectRequestClient>()
    .AddKeycloakService(config.GetSection("Keycloak:ServiceAccount"));

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new HeaderApiVersionReader("api-version");
    // options.DefaultApiVersion = new ApiVersion(1, 0);
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
builder.Services.AddMemoryCache();

builder.Services.AddCors(options =>
    options.AddPolicy(name: "CorsPolicy",
        cfg =>
        {
            cfg.AllowAnyHeader();
            cfg.AllowAnyMethod();
            cfg.WithOrigins(builder.Configuration["AllowedCORS"] ?? "");
        }));

builder.Services.AddSignalR(options =>
    {
        options.EnableDetailedErrors = signalROptions.EnableDetailedErrors;
    })
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.DefaultIgnoreCondition = jsonSerializerOptions.DefaultIgnoreCondition;
        options.PayloadSerializerOptions.PropertyNameCaseInsensitive = jsonSerializerOptions.PropertyNameCaseInsensitive;
        options.PayloadSerializerOptions.PropertyNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy;
        options.PayloadSerializerOptions.WriteIndented = jsonSerializerOptions.WriteIndented;
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // options.PayloadSerializerOptions.Converters.Add(new Int32ToStringJsonConverter());
    });

builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger(options =>
{
    options.RouteTemplate = config.GetValue<string>("Swagger:RouteTemplate");
});
app.UseSwaggerUI(options =>
{
    var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in apiVersionProvider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(String.Format(config.GetValue<string>("Swagger:EndpointPath") ?? "", description.GroupName), description.GroupName);
    }
    options.RoutePrefix = config.GetValue<string>("Swagger:RoutePrefix");
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

app.UsePathBase(config.GetValue<string>("BaseUrl"));
app.UseForwardedHeaders();

app.UseMiddleware(typeof(ErrorHandlingMiddleware));
app.UseMiddleware(typeof(ResponseTimeMiddleware));

// app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");

app.UseMiddleware(typeof(LogRequestMiddleware));

app.UseAuthentication();
app.UseAuthorization();

app.UseMetricServer();
app.UseHttpMetrics();

app.MapMetrics().RequireAuthorization();

app.MapControllers();

if (signalROptions.EnableKafkaBackPlane)
    app.MapHub<MessageHub>(signalROptions.HubPath);

app.Run();
