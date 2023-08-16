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

    /// <summary>
    /// Stops tracking all currently tracked entities.
    /// Microsoft.EntityFrameworkCore.DbContext is designed to have a short lifetime where a new instance is created for each unit-of-work. This manner means all tracked entities are discarded when the context is disposed at the end of each unit-of-work. However, clearing all tracked entities using this method may be useful in situations where creating a new context instance is not practical.
    /// This method should always be preferred over detaching every tracked entity. Detaching entities is a slow process that may have side effects. This method is much more efficient at clearing all tracked entities from the context.
    /// Note that this method does not generate Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.StateChanged events since entities are not individually detached.
    /// </summary>
    public void ClearChangeTracker()
    {
        this.Context.ChangeTracker.Clear();
    }
    #endregion
}
