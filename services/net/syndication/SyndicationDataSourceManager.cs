using System.Text.Json;
using Microsoft.Extensions.Logging;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Syndication.Config;

namespace TNO.Services.Syndication;

/// <summary>
/// SyndicationDataSourceManager class, provides a way to manage the syndication ingestion process for this data source.
/// </summary>
public class SyndicationDataSourceManager : DataSourceManager<SyndicationOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationDataSourceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="logger"></param>
    public SyndicationDataSourceManager(DataSourceModel dataSource, IIngestAction<SyndicationOptions> action, IApiService api, ILogger<IDataSourceManager> logger) : base(dataSource, action, api, logger)
    {
    }
    #endregion

    #region Methods

    /// <summary>
    /// Verify that the specified data source ingestion action should be run.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    public override bool VerifyDataSource(DataSourceModel dataSource)
    {
        if (!dataSource.Connection.ContainsKey("url")) return false;

        var url = (JsonElement)dataSource.Connection["url"];

        if (url.ValueKind == JsonValueKind.String)
            return !String.IsNullOrWhiteSpace(url.GetString());

        return false;
    }
    #endregion
}
