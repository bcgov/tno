using System.Security.Claims;
using TNO.API.Areas.Admin.Models.User;

namespace TNO.API.Keycloak;

/// <summary>
/// IKeycloakHelper interface, provides helper methods to manage and sync Keycloak.
/// </summary>
public interface IKeycloakHelper
{
    #region Methods
    /// <summary>
    /// Sync Keycloak 'Key' value in users, groups, and roles with the TNO users, roles, and claims.
    /// </summary>
    /// <returns></returns>
    Task SyncAsync();

    /// <summary>
    /// Activate the user with TNO and Keycloak.
    /// If the user doesn't currently exist in TNO, activate a new user by adding them to TNO.
    /// If the user exists in TNO, activate user by linking to Keycloak and updating Keycloak.
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    Task<Entities.User?> ActivateAsync(ClaimsPrincipal principal);

    /// <summary>
    /// Update the user in keycloak linked to this user.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<UserModel> UpdateUserAsync(UserModel model);

    /// <summary>
    /// Update the specified user with the specified roles.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    Task<string[]> UpdateUserRolesAsync(Guid key, string[] roles);

    /// <summary>
    /// Delete the user from keycloak linked to the specified 'entity'.
    /// If the user 'Key' is not linked it will do nothing.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task DeleteUserAsync(Entities.User model);
    #endregion
}
