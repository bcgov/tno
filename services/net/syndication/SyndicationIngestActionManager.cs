using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MMI.SmtpEmail;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Models.Extensions;
using TNO.Services.Actions.Managers;
using TNO.Services.Syndication.Config;

namespace TNO.Services.Syndication;

/// <summary>
/// SyndicationIngestActionManager class, provides a way to manage the syndication ingestion process for this data source.
/// </summary>
public class SyndicationIngestActionManager : IngestActionManager<SyndicationOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationIngestActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="api"></param>
    /// <param name="emailService"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public SyndicationIngestActionManager(
        IngestModel ingest,
        IApiService api,
        IEmailService emailService,
        IIngestAction<SyndicationOptions> action,
        IOptions<SyndicationOptions> options,
        ILogger<IServiceActionManager> logger)
        : base(ingest, api, emailService, action, options, logger)
    {
    }
    #endregion

    #region Methods

    /// <summary>
    /// Verify that the specified data source ingestion action should be run.
    /// </summary>
    /// <returns></returns>
    public override bool VerifyIngest()
    {
        var url = this.Ingest.GetConfigurationValue("url");
        return !String.IsNullOrWhiteSpace(url);
    }
    #endregion
}
