using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Actions.Managers;
using TNO.Services.Content.Config;

namespace TNO.Services.Content;

public class ContentActionManager : ServiceActionManager<ContentOptions>
{
    #region Properties
    #endregion

    #region ContentActionManager
    /// <summary>
    /// Creates a new instance of a ContentActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="manager"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentActionManager(
        IApiService api,
        IServiceAction<ContentOptions> action,
        IOptions<ContentOptions> options,
        ILogger<ContentActionManager> logger)
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
