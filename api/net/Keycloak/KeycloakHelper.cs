using System.Security.Claims;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Admin.Models.User;
using TNO.Core.Exceptions;
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
    private readonly Config.KeycloakOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KeycloakHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="keycloakService"></param>
    /// <param name="userService"></param>
    /// <param name="options"></param>
    public KeycloakHelper(IKeycloakService keycloakService, IUserService userService, IOptions<Config.KeycloakOptions> options)
    {
        _keycloakService = keycloakService;
        _userService = userService;
        _options = options.Value;
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
            var kUser = (await _keycloakService.GetUsersAsync(0, 10, new UserFilter() { Username = user.Username })).FirstOrDefault(u => u.Username == user.Username);
            if (kUser != null && kUser.Id.ToString() != user.Key)
            {
                await AddOrUpdateUserAsync(user, kUser);
            }
        }

        var kUsers = await _keycloakService.GetUsersAsync(0, 100);
        foreach (var kUser in kUsers)
        {
            // If the user has a matching key we assume that it has already been synced.
            var user = users.FirstOrDefault(u => u.Key == kUser.Id.ToString());
            if (user == null)
            {
                user = users.FirstOrDefault(u => u.Username == kUser.Username);
                if (user != null)
                {
                    // The user exists in the database but needs to be synced with Keycloak.
                    await AddOrUpdateUserAsync(user, kUser);
                }
                else
                {
                    // The user does not exist in the database and will need to be added.
                    if (!String.IsNullOrWhiteSpace(kUser.Username))
                    {
                        await AddOrUpdateUserAsync(new Entities.User(kUser.Username, kUser.Email ?? "", kUser.Id.ToString()), kUser);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Update the user in the database with the specified keycloak user information.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="kUser"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    private async Task AddOrUpdateUserAsync(Entities.User user, TNO.Keycloak.Models.UserModel kUser)
    {
        if (!_options.ClientId.HasValue) throw new ConfigurationException("Keycloak clientId has not been configured");

        user.Key = kUser.Id.ToString();
        user.Email = kUser.Email ?? user.Email;
        user.FirstName = kUser.FirstName ?? user.FirstName;
        user.LastName = kUser.LastName ?? user.LastName;
        user.EmailVerified = kUser.EmailVerified ?? false;
        user.IsEnabled = kUser.Enabled;
        var displayName = kUser.Attributes?["displayName"]?.FirstOrDefault();
        user.DisplayName = displayName ?? user.DisplayName;

        // Fetch the roles for the user
        var roles = await _keycloakService.GetUserClientRolesAsync(kUser.Id, _options.ClientId.Value);
        user.Roles = String.Join(",", roles.Select(r => $"[{r.Name?.ToLower()}]"));

        if (user.Id == 0)
            _userService.AddAndSave(user);
        else
            _userService.UpdateAndSave(user);
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
        if (!_options.ClientId.HasValue) throw new ConfigurationException("Keycloak clientId has not been configured");

        var key = new Guid(principal.GetKey() ?? Guid.Empty.ToString());
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

            // Fetch the roles for the user
            var roles = await _keycloakService.GetUserClientRolesAsync(key, _options.ClientId.Value);

            if (user == null)
            {
                var kUser = await _keycloakService.GetUserAsync(key);
                if (kUser == null) throw new InvalidOperationException("The user does not exist in keycloak");

                // Add the user to the database.
                user = _userService.AddAndSave(new Entities.User(username, email, key.ToString())
                {
                    DisplayName = kUser.Attributes?["displayName"].FirstOrDefault() ?? principal.GetDisplayName() ?? "",
                    FirstName = kUser.FirstName ?? principal.GetFirstName() ?? "",
                    LastName = kUser.LastName ?? principal.GetLastName() ?? "",
                    IsEnabled = kUser.Enabled,
                    EmailVerified = kUser.EmailVerified ?? false,
                    IsSystemAccount = false,
                    Status = Entities.UserStatus.Activated,
                    LastLoginOn = DateTime.UtcNow,
                    Roles = String.Join(",", roles.Select(r => $"[{r.Name?.ToLower()}]"))
                });
            }
            else if (user != null)
            {
                // Update the user in the database and reference the keycloak uid.
                // The user was created in TNO initially, but now the user has logged in and activated their account.
                user.Key = key.ToString();
                user.Username = username;
                user.Email = email;
                user.FirstName = principal.GetFirstName() ?? "";
                user.LastName = principal.GetLastName() ?? "";
                user.LastLoginOn = DateTime.UtcNow;
                user.Status = Entities.UserStatus.Approved;
                user.Roles = String.Join(",", roles.Select(r => $"[{r.Name?.ToLower()}]"));
                var model = await UpdateUserAsync(new UserModel(user));
                return (Entities.User)model;
            }
        }
        else
        {
            user.LastLoginOn = DateTime.UtcNow;
            _userService.UpdateAndSave(user);
        }

        return user;
    }

    /// <summary>
    /// Update the user in TNO and keycloak linked to the specified 'user'.
    /// If the user 'Key' is not linked it will do nothing.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<UserModel> UpdateUserAsync(UserModel model)
    {
        var user = _userService.UpdateAndSave((Entities.User)model);
        var result = new UserModel(user);
        if (Guid.TryParse(user.Key, out Guid key))
        {
            var kUser = await _keycloakService.GetUserAsync(key);
            if (kUser != null)
            {
                // Update attributes.
                if (kUser.Attributes == null) kUser.Attributes = new Dictionary<string, string[]>();
                kUser.Attributes["displayName"] = new[] { user.DisplayName };
                kUser.EmailVerified = user.EmailVerified;
                kUser.Enabled = user.IsEnabled;
                await _keycloakService.UpdateUserAsync(kUser);

                result.Roles = await UpdateUserRolesAsync(key, model.Roles.ToArray());
            }
        }

        return result;
    }

    /// <summary>
    /// Update the specified user with the specified roles.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    public async Task<string[]> UpdateUserRolesAsync(Guid key, string[] roles)
    {
        if (!_options.ClientId.HasValue) throw new ConfigurationException("Keycloak clientId has not been configured");

        var allRoles = await _keycloakService.GetRolesAsync(_options.ClientId.Value);
        var addRoles = allRoles?.Where(r => roles.Contains(r.Name))?.ToArray() ?? Array.Empty<TNO.Keycloak.Models.RoleModel>();
        var currentRoles = await _keycloakService.GetUserClientRolesAsync(key, _options.ClientId.Value);
        var removeRoles = currentRoles.Where(r => !roles.Contains(r.Name)).ToArray() ?? Array.Empty<TNO.Keycloak.Models.RoleModel>();

        if (addRoles.Length > 0)
            await _keycloakService.AddUserClientRolesAsync(key, _options.ClientId.Value, addRoles);
        if (removeRoles.Length > 0)
            await _keycloakService.RemoveUserClientRolesAsync(key, _options.ClientId.Value, removeRoles);

        var result = await _keycloakService.GetUserClientRolesAsync(key, _options.ClientId.Value);
        return result?.Select(r => r.Name!).ToArray() ?? Array.Empty<string>();
    }

    /// <summary>
    /// Delete the user from TNO and keycloak linked to the specified 'entity'.
    /// If the user 'Key' is not linked it will do nothing.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public async Task DeleteUserAsync(Entities.User entity)
    {
        _userService.DeleteAndSave(entity);
        if (Guid.TryParse(entity.Key, out Guid key)) await _keycloakService.DeleteUserAsync(key);
    }
    #endregion
}
