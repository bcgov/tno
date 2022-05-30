using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Models.Extensions;
using TNO.Services.Capture.Config;
using TNO.Services.Command;

namespace TNO.Services.Capture;

/// <summary>
/// CaptureDataSourceManager class, provides a way to manage the capture ingestion process for this data source.
/// </summary>
public class CaptureDataSourceManager : CommandDataSourceManager<CaptureOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a CaptureDataSourceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public CaptureDataSourceManager(DataSourceModel dataSource, IApiService api, IIngestAction<CaptureOptions> action, IOptions<CaptureOptions> options)
        : base(dataSource, api, action, options)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Verify that the specified data source ingestion action should be run.
    /// </summary>
    /// <returns></returns>
    public override bool VerifyDataSource()
    {
        var url = this.DataSource.GetConnectionValue("url");
        return !String.IsNullOrWhiteSpace(url);
    }
    #endregion
}
