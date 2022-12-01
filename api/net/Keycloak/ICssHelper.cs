using System.Security.Claims;

namespace TNO.API.CSS;

/// <summary>
/// ICSSHelper interface, provides helper methods to manage and sync CSS.
/// </summary>
public interface ICssHelper
{
    #region Methods
    /// <summary>
    /// Sync CSS 'Key' value in users, groups, and roles with the TNO users, roles, and claims.
    /// </summary>
    /// <returns></returns>
    Task SyncAsync();

    /// <summary>
    /// Activate the user with TNO and CSS.
    /// If the user doesn't currently exist in TNO, activate a new user by adding them to TNO.
    /// If the user exists in TNO, activate user by linking to CSS and updating CSS.
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    Task<Entities.User?> ActivateAsync(ClaimsPrincipal principal);

    /// <summary>
    /// Update the specified user with the specified roles.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    Task<string[]> UpdateUserRolesAsync(string username, string[] roles);

    /// <summary>
    /// Delete the user from keycloak linked to the specified 'entity'.
    /// If the user 'Key' is not linked it will do nothing.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task DeleteUserAsync(Entities.User model);
    #endregion
}
