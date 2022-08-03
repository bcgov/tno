using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Actions.Managers;
using TNO.Services.Image.Config;

namespace TNO.Services.Image;

/// <summary>
/// ImageDataSourceManager class, provides a way to manage the image ingestion process for this data source.
/// </summary>
public class ImageDataSourceManager : DataSourceIngestManager<ImageOptions>
{
    #region Variables
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImageDataSourceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ImageDataSourceManager(DataSourceModel dataSource, IApiService api, IIngestAction<ImageOptions> action, IOptions<ImageOptions> options, ILogger<ImageDataSourceManager> logger)
        : base(dataSource, api, action, options)
    {
        _logger = logger;
    }
    #endregion

    #region Methods
    #endregion
}
