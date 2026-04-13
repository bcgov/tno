using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MMI.SmtpEmail;
using TNO.Services.Managers;
using TNO.Services.Syndication.Config;

namespace TNO.Services.Syndication;

/// <summary>
/// SyndicationManager class, provides a way to manage the syndication service.
/// </summary>
public class SyndicationManager : IngestManager<SyndicationIngestActionManager, SyndicationOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="api"></param>
    /// <param name="emailService"></param>
    /// <param name="smtpOptions"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SyndicationManager(
        IServiceProvider serviceProvider,
        IApiService api,
        IEmailService emailService,
        IOptions<SmtpOptions> smtpOptions,
        IngestManagerFactory<SyndicationIngestActionManager, SyndicationOptions> factory,
        IOptions<SyndicationOptions> options,
        ILogger<SyndicationManager> logger)
        : base(serviceProvider, api, emailService, smtpOptions, factory, options, logger)
    {
    }
    #endregion
}
