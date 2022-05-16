using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Actions.Managers;
using TNO.Services.NLP.Config;

namespace TNO.Services.NLP;

public class NLPActionManager : ServiceActionManager<NLPOptions>
{
    #region Properties
    #endregion

    #region NLPActionManager
    /// <summary>
    /// Creates a new instance of a NLPActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="manager"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public NLPActionManager(
        IApiService api,
        IServiceAction<NLPOptions> action,
        IOptions<NLPOptions> options,
        ILogger<NLPActionManager> logger)
        : base(action)
    {
    }
    #endregion

    #region Methods

    public override Task RecordFailureAsync()
    {
        throw new NotImplementedException();
    }

    public override Task RecordSuccessAsync()
    {
        throw new NotImplementedException();
    }
    #endregion
}
