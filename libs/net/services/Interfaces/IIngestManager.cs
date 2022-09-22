using TNO.API.Areas.Services.Models.Ingest;

namespace TNO.Services;

/// <summary>
/// IIngestManager interface, provides a way to manage several ingest schedules.
/// It will fetch all ingests for the configured media types.
/// It will ensure all ingests are being run based on their schedules.
/// </summary>
public interface IIngestManager : IServiceManager
{
    #region Properties
    #endregion

    #region Methods
    public Task<IEnumerable<IngestModel>> GetIngestsAsync();
    #endregion
}
