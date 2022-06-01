using System.Security.Claims;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Keycloak;

/// <summary>
/// KeycloakHelper class, provides helper methods to manage and sync Keycloak.
/// </summary>
public class KeycloakHelper : IKeycloakHelper
{
    #region Variables
    private readonly IKeycloakService _keycloakService;
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IClaimService _claimService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KeycloakHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="keycloakService"></param>
    /// <param name="userService"></param>
    /// <param name="roleService"></param>
    /// <param name="claimService"></param>
    public KeycloakHelper(IKeycloakService keycloakService, IUserService userService, IRoleService roleService, IClaimService claimService)
    {
        _keycloakService = keycloakService;
        _userService = userService;
        _roleService = roleService;
        _claimService = claimService;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Sync Keycloak 'Key' value in users, groups, and roles with the TNO users, roles, and claims.
    /// </summary>
    /// <returns></returns>
    public async Task SyncAsync()
    {
        var users = _userService.FindAll();
        foreach (var user in users)
        {
            var kuser = (await _keycloakService.GetUsersAsync(0, 10, user.Username)).FirstOrDefault(u => u.Username == user.Username);
            if (kuser != null && kuser.Id != user.Key)
            {
                user.Key = kuser.Id;
                user.Email = kuser.Email ?? user.Email;
                user.FirstName = kuser.FirstName ?? user.FirstName;
                user.LastName = kuser.LastName ?? user.LastName;
                user.EmailVerified = kuser.EmailVerified ?? false;
                user.IsEnabled = kuser.Enabled;
                var displayName = kuser.Attributes?["displayName"]?.FirstOrDefault();
                user.DisplayName = displayName ?? user.DisplayName;
                _userService.Update(user);
            }
        }

        var roles = _roleService.FindAll();
        foreach (var role in roles)
        {
            var kgroup = (await _keycloakService.GetGroupsAsync(0, 10, role.Name)).FirstOrDefault(g => g.Name == role.Name);
            if (kgroup != null && kgroup.Id != role.Key)
            {
                role.Key = kgroup.Id;
                _roleService.Update(role);
            }
        }

        var claims = _claimService.FindAll();
        foreach (var claim in claims)
        {
            var krole = (await _keycloakService.GetRolesAsync()).FirstOrDefault(r => r.Name == claim.Name);
            if (krole != null && krole.Id != claim.Key)
            {
                claim.Key = krole.Id;
                _claimService.Update(claim);
            }
        }
    }

    /// <summary>
    /// Activate the user with TNO and Keycloak.
    /// If the user doesn't currently exist in TNO, activate a new user by adding them to TNO.
    /// If the user exists in TNO, activate user by linking to Keycloak and updating Keycloak.
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public async Task<Entities.User?> ActivateAsync(ClaimsPrincipal principal)
    {
        var key = principal.GetUid();
        var username = principal.GetUsername() ?? throw new InvalidOperationException("Username is required but missing from token");
        var user = _userService.FindByKey(key);

        // If user doesn't exist, add them to the database.
        if (user == null)
        {
            var email = principal.GetEmail() ?? throw new InvalidOperationException("Email is required but missing from token");

            // Check if the user has been manually added by their email address.
            var users = _userService.FindByEmail(email);

            // If only one account has the email, we can assume it's a preapproved user.
            // However if it isn't we need to see if there is a match for the username instead (which is unlikely).
            if (users.Count() == 1) user = users.First();
            else user = _userService.FindByUsername(username);

            if (user == null)
            {
                var kuser = await _keycloakService.GetUserAsync(key);
                if (kuser == null) throw new InvalidOperationException("The user does not exist in keycloak");

                // Add the user to the database.
                user = _userService.Add(new Entities.User(username, email, key)
                {
                    DisplayName = kuser.Attributes?["displayName"].FirstOrDefault() ?? principal.GetDisplayName() ?? "",
                    FirstName = kuser.FirstName ?? principal.GetFirstName() ?? "",
                    LastName = kuser.LastName ?? principal.GetLastName() ?? "",
                    IsEnabled = kuser.Enabled,
                    EmailVerified = kuser.EmailVerified ?? false,
                    IsSystemAccount = false,
                    Status = Entities.UserStatus.Activated,
                    LastLoginOn = DateTime.UtcNow,
                });
            }
            else if (user != null)
            {
                // Update the user in the database and reference the keycloak uid.
                // The user was created in TNO intially, but now the user has logged in and activited their account.
                user.Key = key;
                user.Username = username;
                user.Email = email;
                user.FirstName = principal.GetFirstName() ?? "";
                user.LastName = principal.GetLastName() ?? "";
                user.LastLoginOn = DateTime.UtcNow;
                user.Status = Entities.UserStatus.Approved;
                user = await UpdateUserAsync(user);
            }
        }
        else
        {
            user.LastLoginOn = DateTime.UtcNow;
            _userService.Update(user);
        }

        return user;
    }

    /// <summary>
    /// Update the user in TNO and keycloak linked to the specified 'user'.
    /// If the user 'Key' is not linked it will do nothing.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task<Entities.User?> UpdateUserAsync(Entities.User entity)
    {
        var user = _userService.Update(entity);
        if (user.Key != Guid.Empty)
        {
            var kuser = await _keycloakService.GetUserAsync(user.Key);
            if (kuser != null)
            {
                // Update attributes.
                if (kuser.Attributes == null) kuser.Attributes = new Dictionary<string, string[]>();
                kuser.Attributes["displayName"] = new[] { user.DisplayName };
                kuser.EmailVerified = user.EmailVerified;
                kuser.Enabled = user.IsEnabled;
                await _keycloakService.UpdateUserAsync(kuser);

                var roles = _roleService.FindAll();

                // Update groups.
                var userGroups = await _keycloakService.GetUserGroupsAsync(user.Key);
                foreach (var role in user.RolesManyToMany.Where(r => r.Role?.Key != Guid.Empty).Select(r => r.Role!))
                {
                    if (!userGroups.Any(ug => ug.Id == role.Key))
                        await _keycloakService.AddGroupToUserAsync(user.Key, role.Key);
                }
                foreach (var group in userGroups.Where(ug => !user.RolesManyToMany.Select(r => r.Role?.Key).Any(k => k == ug.Id)))
                {
                    // Only remove TNO roles from the keycloak groups.
                    if (roles.Any(r => r.Name == group.Name))
                        await _keycloakService.RemoveGroupFromUserAsync(user.Key, group.Id);
                }
            }
        }

        return user;
    }

    /// <summary>
    /// Delete the user from TNO and keycloak linked to the specified 'entity'.
    /// If the user 'Key' is not linked it will do nothing.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task DeleteUserAsync(Entities.User entity)
    {
        _userService.Delete(entity);
        if (entity.Key != Guid.Empty) await _keycloakService.DeleteUserAsync(entity.Key);
    }
    #endregion
}
