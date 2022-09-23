using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Image.Config;
using TNO.API.Areas.Services.Models.Ingest;

namespace TNO.Services.Image;

/// <summary>
/// ImageManager class, provides a way to manage the image service.
/// </summary>
public class ImageManager : IngestManager<ImageIngestActionManager, ImageOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImageManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageManager(
        IApiService api,
        IngestManagerFactory<ImageIngestActionManager, ImageOptions> factory,
        IOptions<ImageOptions> options,
        ILogger<ImageManager> logger)
        : base(api, factory, options, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the data sources for the image services.
    /// </summary>
    /// <returns></returns>
    public override async Task<IEnumerable<IngestModel>> GetIngestsAsync()
    {
        var dataSources = await base.GetIngestsAsync();

        return dataSources.Where(ds => IsImage(ds));
    }

    /// <summary>
    /// Determine if the data source of the correct ingest type for this service.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    private bool IsImage(IngestModel dataSource)
    {
        return _options.GetIngestTypes().Contains(dataSource.IngestType?.Name);
    }
    #endregion
}
