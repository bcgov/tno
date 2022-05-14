using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TNO.Core.Http;
using TNO.Core.Http.Configuration;
using TNO.Services.Config;
using TNO.Services.Controllers;
using TNO.Services.Syndication.Config;

namespace TNO.Services.Syndication;

/// <summary>
/// Program static class, provides a console application that runs service.
/// </summary>
public class Program
{
    #region Variables
    private readonly ILogger _logger;
    #endregion

    #region Properties
    /// <summary>
    /// get - Program configuration.
    /// </summary>
    public IConfiguration Configuration { get; private set; }

    /// <summary>
    /// get - The service provider.
    /// </summary>
    public WebApplication App { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Program object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public Program(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        this.Configuration = Configure(builder).Build();
        ConfigureServices(builder.Services);
        this.App = builder.Build();
        _logger = this.App.Services.GetRequiredService<ILogger<Program>>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Console main method.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static int Main(string[] args)
    {
        var program = new Program(args);
        return program.Run();
    }

    /// <summary>
    /// Configure application.
    /// </summary>
    /// <returns></returns>
    private static IConfigurationBuilder Configure(WebApplicationBuilder builder)
    {
        DotNetEnv.Env.Load();
        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        return builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables();
    }

    /// <summary>
    /// Configure dependency injection.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private void ConfigureServices(IServiceCollection services)
    {
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = !String.IsNullOrWhiteSpace(this.Configuration["Serialization:Json:DefaultIgnoreCondition"]) ? Enum.Parse<JsonIgnoreCondition>(this.Configuration["Serialization:Json:DefaultIgnoreCondition"]) : JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = !String.IsNullOrWhiteSpace(this.Configuration["Serialization:Json:PropertyNameCaseInsensitive"]) && Boolean.Parse(this.Configuration["Serialization:Json:PropertyNameCaseInsensitive"]),
            PropertyNamingPolicy = this.Configuration["Serialization:Json:PropertyNamingPolicy"] == "CamelCase" ? JsonNamingPolicy.CamelCase : null,
            WriteIndented = !string.IsNullOrWhiteSpace(this.Configuration["Serialization:Json:WriteIndented"]) && Boolean.Parse(this.Configuration["Serialization:Json:WriteIndented"])
        };

        services
            .AddSingleton<IConfiguration>(this.Configuration)
            .Configure<ProducerConfig>(this.Configuration.GetSection("Kafka"))
            .Configure<IngestServiceOptions>(this.Configuration.GetSection("IngestService"))
            .Configure<SyndicationOptions>(this.Configuration.GetSection("IngestService"))
            .Configure<AuthClientOptions>(this.Configuration.GetSection("Auth:Keycloak"))
            .Configure<OpenIdConnectOptions>(this.Configuration.GetSection("Auth:OIDC"))
            .Configure<JsonSerializerOptions>(options =>
            {
                options.DefaultIgnoreCondition = jsonSerializerOptions.DefaultIgnoreCondition;
                options.PropertyNameCaseInsensitive = jsonSerializerOptions.PropertyNameCaseInsensitive;
                options.PropertyNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy;
                options.WriteIndented = jsonSerializerOptions.WriteIndented;
                options.Converters.Add(new JsonStringEnumConverter());
            })
            .AddLogging(options =>
            {
                options.AddConfiguration(this.Configuration.GetSection("Logging"));
                options.AddConsole();
            })
            .AddTransient<JwtSecurityTokenHandler>()
            .AddTransient<IKafkaProducerService, KafkaProducerService>()
            .AddTransient<IHttpRequestClient, HttpRequestClient>()
            .AddTransient<IOpenIdConnectRequestClient, OpenIdConnectRequestClient>()
            .AddTransient<IApiService, ApiService>()
            .AddTransient<IIngestAction<SyndicationOptions>, SyndicationAction>()
            .AddTransient<DataSourceManagerFactory<SyndicationDataSourceManager, SyndicationOptions>>()
            .AddSingleton<IServiceManager, SyndicationService>();

        services.AddOptions<SyndicationOptions>()
            .Bind(this.Configuration.GetSection("Syndication"))
            .ValidateDataAnnotations();

        services.AddOptions<AuthClientOptions>()
            .Bind(this.Configuration.GetSection("Auth:Keycloak"))
            .ValidateDataAnnotations();

        services.AddOptions<OpenIdConnectOptions>()
            .Bind(this.Configuration.GetSection("Auth:OIDC"))
            .ValidateDataAnnotations();

        services.AddHttpClient(typeof(SyndicationService).FullName, client => { });

        // API services
        services.AddMvcCore()
            .AddApplicationPart(typeof(HealthController).Assembly);
        services.AddHttpContextAccessor()
            .AddControllers(options =>
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
            });
    }

    /// <summary>
    /// Run the ingestion service.
    /// </summary>
    /// <returns></returns>
    public int Run()
    {
        try
        {
            _logger.LogInformation("Service started");
            return Task.WaitAny(RunApiAsync(), RunServiceAsync());
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An unhandled error has occurred.");
            return 1;
        }
        finally
        {
            _logger.LogInformation("Service stopped");
        }
    }

    /// <summary>
    /// Run the service.
    /// </summary>
    /// <returns></returns>
    public async Task RunServiceAsync()
    {
        var service = this.App.Services.GetRequiredService<IServiceManager>();
        await service.RunAsync();
    }

    /// <summary>
    /// Run the API.
    /// </summary>
    private async Task RunApiAsync()
    {
        // Configure the HTTP request pipeline.
        if (this.App.Environment.IsDevelopment())
        {
            this.App.UseDeveloperExceptionPage();
        }

        this.App.UsePathBase(this.Configuration.GetValue<string>("BaseUrl"));
        this.App.UseForwardedHeaders();

        // app.UseHttpsRedirection();
        this.App.UseRouting();
        this.App.UseCors();

        this.App.MapControllers();

        await this.App.RunAsync();
    }
    #endregion
}
