using Microsoft.Extensions.Options;
using System.Text.Json;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Actions.Managers;
using TNO.Services.CPNews.Config;

namespace TNO.Services.CPNews;

/// <summary>
/// CPNewsDataSourceManager class, provides a way to manage the cobews ingestion process for this data source.
/// </summary>
public class CPNewsDataSourceManager : DataSourceIngestManager<CPNewsOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a CPNewsDataSourceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public CPNewsDataSourceManager(DataSourceModel dataSource, IApiService api, IIngestAction<CPNewsOptions> action, IOptions<CPNewsOptions> options)
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
