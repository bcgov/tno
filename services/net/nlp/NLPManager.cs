using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.NLP.Config;

namespace TNO.Services.NLP;

public class NLPManager : ServiceManager<NLPOptions>
{
    #region Properties
    private readonly IServiceActionManager _manager;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NLPManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="manager"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public NLPManager(
        IApiService api,
        IServiceActionManager manager,
        IOptions<NLPOptions> options,
        ILogger<NLPManager> logger)
        : base(api, options, logger)
    {
        _manager = manager;
    }
    #endregion

    #region Methods
    public override async Task RunAsync()
    {
        await _manager.RunAsync();
    }
    #endregion
}
