using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Clip.Config;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Models.Extensions;

namespace TNO.Services.Clip;

/// <summary>
/// ClipManager class, provides a way to manage the clip service.
/// </summary>
public class ClipManager : IngestManager<ClipIngestActionManager, ClipOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ClipManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ClipManager(
        IApiService api,
        IngestManagerFactory<ClipIngestActionManager, ClipOptions> factory,
        IOptions<ClipOptions> options,
        ILogger<ClipManager> logger)
        : base(api, factory, options, logger)
    {
    }

    /// <summary>
    /// Get the data sources for the clip services.
    /// Only data sources of serviceType=stream.
    /// </summary>
    /// <returns></returns>
    public override async Task<IEnumerable<IngestModel>> GetIngestsAsync()
    {
        var ingests = await base.GetIngestsAsync();
        var serviceType = !String.IsNullOrWhiteSpace(this.Options.ServiceType) ? this.Options.ServiceType : "clip";
        var hostname = System.Environment.GetEnvironmentVariable("HOSTNAME");
        return ingests.Where(i =>
            i.GetConfigurationValue("serviceType") == serviceType &&
            (String.IsNullOrWhiteSpace(i.GetConfigurationValue("hostname")) ||
                i.GetConfigurationValue("hostname") == hostname));
    }
    #endregion
}
