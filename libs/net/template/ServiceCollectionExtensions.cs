using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorEngineCore;
using TNO.TemplateEngine.Config;

namespace TNO.TemplateEngine;

/// <summary>
/// ServiceCollectionExtensions static class, provides extension methods for IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the template engine to dependency injection service provider.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddTemplateEngine(this IServiceCollection services, IConfiguration config)
    {
        return services
            .Configure<ChartsOptions>(config.GetSection("Charts"))
            .Configure<TemplateOptions>(config.GetSection("Reporting"))
            .AddScoped<IRazorEngine, RazorEngine>()
            .AddScoped<ITemplateEngine<Models.Notifications.NotificationEngineContentModel>, TemplateEngine<Models.Notifications.NotificationEngineContentModel>>()
            .AddScoped<ITemplateEngine<Models.Reports.ReportEngineContentModel>, TemplateEngine<Models.Reports.ReportEngineContentModel>>()
            .AddScoped<ITemplateEngine<Models.Reports.ReportEngineAVOverviewModel>, TemplateEngine<Models.Reports.ReportEngineAVOverviewModel>>()
            .AddScoped<ITemplateEngine<Models.Reports.ChartEngineContentModel>, TemplateEngine<Models.Reports.ChartEngineContentModel>>()
            .AddScoped<INotificationEngine, NotificationEngine>()
            .AddScoped<IReportEngine, ReportEngine>();
    }

    /// <summary>
    /// Add the template engine to dependency injection service provider.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddTemplateEngineSingleton(this IServiceCollection services, IConfiguration config)
    {
        return services
            .Configure<ChartsOptions>(config.GetSection("Charts"))
            .Configure<TemplateOptions>(config.GetSection("Reporting"))
            .AddSingleton<IRazorEngine, RazorEngine>()
            .AddSingleton<ITemplateEngine<Models.Notifications.NotificationEngineContentModel>, TemplateEngine<Models.Notifications.NotificationEngineContentModel>>()
            .AddSingleton<ITemplateEngine<Models.Reports.ReportEngineContentModel>, TemplateEngine<Models.Reports.ReportEngineContentModel>>()
            .AddSingleton<ITemplateEngine<Models.Reports.ReportEngineAVOverviewModel>, TemplateEngine<Models.Reports.ReportEngineAVOverviewModel>>()
            .AddSingleton<ITemplateEngine<Models.Reports.ChartEngineContentModel>, TemplateEngine<Models.Reports.ChartEngineContentModel>>()
            .AddSingleton<INotificationEngine, NotificationEngine>()
            .AddSingleton<IReportEngine, ReportEngine>();
    }
}
