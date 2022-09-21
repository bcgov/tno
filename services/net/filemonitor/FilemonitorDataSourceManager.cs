using Microsoft.Extensions.Options;
using System.Text.Json;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Actions.Managers;
using TNO.Services.Filemonitor.Config;

namespace TNO.Services.Filemonitor;

/// <summary>
/// SyndicationDataSourceManager class, provides a way to manage the syndication ingestion process for this data source.
/// </summary>
public class FilemonitorDataSourceManager : DataSourceIngestManager<FilemonitorOptions>
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SyndicationDataSourceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public FilemonitorDataSourceManager(DataSourceModel dataSource, IApiService api, IIngestAction<FilemonitorOptions> action, IOptions<FilemonitorOptions> options)
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
        if (!this.DataSource.Connection.ContainsKey("importDir")) return false;

        var importDir = (JsonElement)this.DataSource.Connection["importDir"];

        if (importDir.ValueKind == JsonValueKind.String)
            return !String.IsNullOrWhiteSpace(importDir.GetString());

        return false;
    }
    #endregion
}
