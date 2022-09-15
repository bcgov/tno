using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
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
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public ImageIngestActionManager(IngestModel dataSource, IApiService api, IIngestAction<ImageOptions> action, IOptions<ImageOptions> options)
        : base(dataSource, api, action, options)
    {
    }
    #endregion

    #region Methods
    #endregion
}
