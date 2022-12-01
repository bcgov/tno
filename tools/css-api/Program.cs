using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc.Versioning;
using TNO.Core.Http;
using TNO.CSS.API.Config;
using TNO.Keycloak;

DotNetEnv.Env.Load();
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddControllers();
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddHttpClient()
    .AddTransient<JwtSecurityTokenHandler>()
    .AddTransient<IHttpRequestClient, HttpRequestClient>()
    .AddTransient<IOpenIdConnectRequestClient, OpenIdConnectRequestClient>()
    .Configure<CssOptions>(config.GetSection("Keycloak"))
    .AddKeycloakService(config.GetSection("Keycloak"));


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

var app = builder.Build();

app.UsePathBase(config.GetValue<string>("BaseUrl"));
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) =>
    {
        var sb = new StringBuilder();
        var endpoints = endpointSources.SelectMany(es => es.Endpoints);
        foreach (var endpoint in endpoints)
        {
            if (endpoint is RouteEndpoint routeEndpoint)
            {
                sb.AppendLine(routeEndpoint.DisplayName);
                sb.AppendLine(routeEndpoint.RoutePattern.RawText);
                // sb.AppendLine(String.Join(", ", routeEndpoint.RoutePattern.PathSegments.SelectMany(p => p.Parts.Select(p => p.)));
                sb.AppendLine(String.Join(", ", routeEndpoint.RoutePattern.Parameters.Select(p => p.Name)));
                sb.AppendLine(routeEndpoint.RoutePattern.InboundPrecedence.ToString());
                sb.AppendLine(routeEndpoint.RoutePattern.OutboundPrecedence.ToString());
            }

            var routeNameMetadata = endpoint.Metadata.OfType<Microsoft.AspNetCore.Routing.RouteNameMetadata>().FirstOrDefault();
            sb.AppendLine("RouteName:" + routeNameMetadata?.RouteName);

            var httpMethodsMetadata = endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault();
            sb.AppendLine("Method:" + String.Join(", ", httpMethodsMetadata?.HttpMethods ?? Array.Empty<string>())); // [GET, POST, ...]
            sb.AppendLine();

            // There are many more metadata types available...
        }
        return sb.ToString();
    });
}

app.Run();
