using System.Text.Json;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Actions.Managers;
using TNO.Services.Syndication.Config;

namespace TNO.Services.Syndication;

/// <summary>
/// SyndicationDataSourceManager class, provides a way to manage the syndication ingestion process for this data source.
/// </summary>
public class SyndicationDataSourceManager : DataSourceIngestManager<SyndicationOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationDataSourceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    public SyndicationDataSourceManager(DataSourceModel dataSource, IApiService api, IIngestAction<SyndicationOptions> action)
        : base(dataSource, api, action)
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
