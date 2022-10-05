using Microsoft.Extensions.Options;
using System.Text.Json;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Models.Extensions;
using TNO.Services.Actions.Managers;
using TNO.Services.FileMonitor.Config;

namespace TNO.Services.FileMonitor;

/// <summary>
/// FileMonitorIngestActionManager class, provides a way to manage the file ingestion process for this ingest.
/// </summary>
public class FileMonitorIngestActionManager : IngestActionManager<FileMonitorOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a FileMonitorIngestActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public FileMonitorIngestActionManager(IngestModel ingest, IApiService api, IIngestAction<FileMonitorOptions> action, IOptions<FileMonitorOptions> options)
        : base(ingest, api, action, options)
    {
    }
    #endregion

    #region Methods

    /// <summary>
    /// Verify that the specified ingest ingestion action should be run.
    /// </summary>
    /// <returns></returns>
    public override bool VerifyIngest()
    {
        var filePattern = this.Ingest.GetConfigurationValue("filePattern");
        return !String.IsNullOrWhiteSpace(filePattern);
    }
    #endregion
}
