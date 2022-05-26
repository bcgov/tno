using Microsoft.Extensions.Options;
using System.Text.Json;
using TNO.API.Areas.Services.Models.DataSource;
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
        if (!this.DataSource.Connection.ContainsKey("url")) return false;

        var url = (JsonElement)this.DataSource.Connection["url"];

        if (url.ValueKind == JsonValueKind.String)
            return !String.IsNullOrWhiteSpace(url.GetString());

        return false;
    }
    #endregion
}
