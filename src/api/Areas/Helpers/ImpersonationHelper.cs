
using System.Security.Claims;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Helpers;

/// <summary>
/// ImpersonationHelper class, provides helper methods for user authorization and impersonation.
/// </summary>
public class ImpersonationHelper : IImpersonationHelper
{
    #region Variables
    private readonly IUserService _userService;
    #endregion

    #region Properties

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
    protected ILogger<IImpersonationHelper> Logger { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ImpersonationHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="userService"></param>
    /// <param name="logger"></param>
    public ImpersonationHelper(
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        IUserService userService,
        ILogger<IImpersonationHelper> logger)
    {
        this.Principal = principal;
        this.Services = serviceProvider;
        _userService = userService;
        this.Logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the currently logged in user, or if they are impersonating get that user instead.
    /// </summary>
    /// <returns></returns>
    public Entities.User GetCurrentUser()
    {
        var username = this.Principal.GetUsername() ?? throw new NotAuthorizedException("Username is missing");
        var user = _userService.FindByUsername(username) ?? throw new NotAuthorizedException($"User [{username}] does not exist");
        var isAdmin = user.Roles.Split(',').Contains($"[{ClientRole.Administrator.GetName()}]");
        var impersonate = user.Preferences.GetElementValue<string?>(".impersonate");

        // If the user is an admin and has a configured impersonation, return that user instead.
        if (isAdmin && !String.IsNullOrWhiteSpace(impersonate))
        {
            user = _userService.FindByUserKey(impersonate) ?? throw new NotAuthorizedException($"User [{impersonate}] does not exist");
        }

        return user;
    }
    #endregion
}
