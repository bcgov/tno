using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MMI.SmtpEmail;

/// <summary>
/// Extension methods for registering the EmailService and related services in the dependency injection container. Provides methods to register using IConfiguration or an options configuration callback, and to register an injectable SmtpClient configured from SmtpOptions. The AddSmtpEmail methods register the IEmailService and configure SmtpOptions, while AddSmtpClient registers an injectable SmtpClient that can be used by the EmailService when it creates its own instance. If you are injecting your own SmtpClient, you do not need to call AddSmtpClient.
/// </summary>
public static class ServicesExtensions
{
    /// <summary>
    /// Register SmtpOptions, an injectable SmtpClient configured from options, and the EmailService.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmtpEmail(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        return services.AddTransient<IEmailService, EmailService>();
    }

    /// <summary>
    /// Register SmtpOptions, an injectable SmtpClient configured from options, and the EmailService.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmtpEmail(this IServiceCollection services, IConfigurationSection configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<SmtpOptions>(configuration);
        return services.AddTransient<IEmailService, EmailService>();
    }

    /// <summary>
    /// Register using an options configuration callback instead of IConfiguration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmtpEmail(this IServiceCollection services, Action<SmtpOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        return services.AddTransient<IEmailService, EmailService>();
    }

    /// <summary>
    /// Register an injectable SmtpClient configured from SmtpOptions. This is used by the EmailService when it creates its own SmtpClient instance. If you are injecting your own SmtpClient, you do not need to call this method.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSmtpClient(this IServiceCollection services)
    {
        services.AddTransient(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<SmtpOptions>>().Value;
            var client = new SmtpClient(opts.Host, opts.Port)
            {
                EnableSsl = opts.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = opts.UseDefaultCredentials
            };

            if (!string.IsNullOrWhiteSpace(opts.Username))
            {
                client.Credentials = new NetworkCredential(opts.Username, opts.Password);
            }

            if (opts.Timeout.HasValue)
                client.Timeout = (int)opts.Timeout.Value.TotalMilliseconds;

            return client;
        });

        return services;
    }
}
