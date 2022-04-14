
using System.Reflection;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TNO.API.Middleware;
using TNO.Core.Converters;
using TNO.DAL;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var builder = WebApplication.CreateBuilder(args);

builder
    .Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
    .AddJsonFile("connectionstrings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"connectionstrings.{env}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);
var config = builder.Configuration;

// Add services to the container.

var jsonSerializerOptions = new JsonSerializerOptions()
{
    DefaultIgnoreCondition = !String.IsNullOrWhiteSpace(config["Serialization:Json:DefaultIgnoreCondition"]) ? Enum.Parse<JsonIgnoreCondition>(config["Serialization:Json:DefaultIgnoreCondition"]) : JsonIgnoreCondition.WhenWritingNull,
    PropertyNameCaseInsensitive = !String.IsNullOrWhiteSpace(config["Serialization:Json:PropertyNameCaseInsensitive"]) && Boolean.Parse(config["Serialization:Json:PropertyNameCaseInsensitive"]),
    PropertyNamingPolicy = config["Serialization:Json:PropertyNamingPolicy"] == "CamelCase" ? JsonNamingPolicy.CamelCase : null,
    WriteIndented = !string.IsNullOrWhiteSpace(config["Serialization:Json:WriteIndented"]) && Boolean.Parse(config["Serialization:Json:WriteIndented"])
};
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.DefaultIgnoreCondition = jsonSerializerOptions.DefaultIgnoreCondition;
    options.PropertyNameCaseInsensitive = jsonSerializerOptions.PropertyNameCaseInsensitive;
    options.PropertyNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy;
    options.WriteIndented = jsonSerializerOptions.WriteIndented;
    options.Converters.Add(new JsonStringEnumConverter());
    options.Converters.Add(new Int32ToStringJsonConverter());
});

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
      options.JsonSerializerOptions.Converters.Add(new Int32ToStringJsonConverter());
  });

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.Authority = config["Keycloak:Authority"];
        options.Audience = config["Keycloak:Audience"];
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            // ValidIssuer = config["Keycloak:Issuer"] ?? options.Authority,
            ValidateIssuer = false,
            ValidateAudience = false,
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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, TNO.API.Config.Swagger.ConfigureSwaggerOptions>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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

builder.Services.AddTNOServices(config);

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger(options =>
    {
        options.RouteTemplate = config.GetValue<string>("Swagger:RouteTemplate");
    });
    app.UseSwaggerUI(options =>
    {
        var apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var description in apiVersionProvider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(String.Format(config.GetValue<string>("Swagger:EndpointPath"), description.GroupName), description.GroupName);
        }
        options.RoutePrefix = config.GetValue<string>("Swagger:RoutePrefix");
    });
}

app.UsePathBase(config.GetValue<string>("BaseUrl"));
app.UseForwardedHeaders();

app.UseMiddleware(typeof(ErrorHandlingMiddleware));
app.UseMiddleware(typeof(ResponseTimeMiddleware));

// app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();

app.UseMiddleware(typeof(LogRequestMiddleware));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
