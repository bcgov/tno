using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace TNO.DAL.Services;

/// <summary>
/// BaseService abstract class, provides the shared properties and methods for services.
/// </summary>
public abstract class BaseService : IBaseService
{
    #region Properties

    /// <summary>
    /// get - The datasource context object.
    /// </summary>
    protected TNOContext Context { get; }

    /// <summary>
    /// get - The user principal claim.
    /// </summary>
    public ClaimsPrincipal Principal { get; }

    /// <summary>
    /// get - The service provider.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// get - The logger.
    /// </summary>
    protected ILogger<BaseService> Logger { get; }
    #endregion

    #region Constructors
    public BaseService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<BaseService> logger)
    {
        this.Context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        this.Principal = principal ?? throw new ArgumentNullException(nameof(principal));
        this.Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Commit the transaction.
    /// </summary>
    /// <returns></returns>
    public int CommitTransaction()
    {
        return this.Context.CommitTransaction();
    }
    #endregion
}
