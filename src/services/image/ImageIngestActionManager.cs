using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Services.Actions.Managers;
using TNO.Services.Image.Config;

namespace TNO.Services.Image;

/// <summary>
/// ImageIngestActionManager class, provides a way to manage the image ingestion process for this data source.
/// </summary>
public class ImageIngestActionManager : IngestActionManager<ImageOptions>
{
    #region Variables
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImageIngestActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="api"></param>
    /// <param name="ches"></param>
    /// <param name="chesOptions"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageIngestActionManager(
        IngestModel dataSource,
        IApiService api,
        IChesService ches,
        IOptions<ChesOptions> chesOptions,
        IIngestAction<ImageOptions> action,
        IOptions<ImageOptions> options,
        ILogger<IServiceActionManager> logger)
        : base(dataSource, api, ches, chesOptions, action, options, logger)
    {
    }
    #endregion

    #region Methods
    #endregion
}
