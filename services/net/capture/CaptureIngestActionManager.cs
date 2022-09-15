using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Models.Extensions;
using TNO.Services.Capture.Config;
using TNO.Services.Command;

namespace TNO.Services.Capture;

/// <summary>
/// CaptureIngestActionManager class, provides a way to manage the capture ingestion process for this data source.
/// </summary>
public class CaptureIngestActionManager : CommandIngestActionManager<CaptureOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a CaptureIngestActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public CaptureIngestActionManager(IngestModel ingest, IApiService api, IIngestAction<CaptureOptions> action, IOptions<CaptureOptions> options)
        : base(ingest, api, action, options)
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
        return !String.IsNullOrWhiteSpace(url) && this.Ingest.IsEnabled;
    }
    #endregion
}
