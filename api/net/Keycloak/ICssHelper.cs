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
    /// <param name="location"></param>
    /// <returns></returns>
    Task<Tuple<Entities.User?, Models.Auth.AccountAuthState>> ActivateAsync(ClaimsPrincipal principal, Models.Auth.LocationModel? location = null);

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

    /// <summary>
    /// Remove the specified location.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="deviceKey"></param>
    void RemoveLocation(ClaimsPrincipal principal, string? deviceKey = null);

    /// <summary>
    /// Keep the specified location and remove all others.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="deviceKey"></param>
    void RemoveOtherLocations(ClaimsPrincipal principal, string? deviceKey = null);
    #endregion
}
