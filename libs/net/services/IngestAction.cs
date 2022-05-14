using Microsoft.Extensions.Options;
using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// IngestAction abstract class, provides a way to configure what action this service performs when a data source is run.
/// </summary>
public abstract class IngestAction<TOptions> : IIngestAction<TOptions>
    where TOptions : IngestServiceOptions
{
    #region Variables
    #endregion

    #region Properties
    /// <summary>
    /// get - The service to communicate with the api.
    /// </summary>
    public IApiService Api { get; private set; }

    /// <summary>
    /// get - The ingest configuration options.
    /// </summary>
    public TOptions Options { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public IngestAction(IApiService api, IOptions<TOptions> options)
    {
        this.Api = api;
        this.Options = options.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the action for the specified data source.
    /// </summary>
    /// <param name="dataSource"></param>
    public abstract Task PerformActionAsync(IDataSourceManager dataSource);
    #endregion
}
