using System.Security.Claims;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.CSS;
using TNO.CSS.Models;
using TNO.Entities;
using TNO.CSS.Extensions;

namespace TNO.API.CSS;

/// <summary>
/// CSSHelper class, provides helper methods to manage and sync CSS.
/// </summary>
public class CssHelper : ICssHelper
{
    #region Variables
    private readonly ICssEnvironmentService _cssService;
    private readonly IUserService _userService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CSSHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="cssService"></param>
    /// <param name="userService"></param>
    public CssHelper(ICssEnvironmentService cssService, IUserService userService)
    {
        _cssService = cssService;
        _userService = userService;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Sync CSS 'Key' value in users, groups, and roles with the TNO users, roles, and claims.
    /// </summary>
    /// <returns></returns>
    public async Task SyncAsync()
    {
        // For each user already in the database, determine if there is a user in keycloak with the same email.
        // If so, update the local user information.
        var dbUsers = _userService.FindAll();
        await SyncKeycloakUsersWithLocal(dbUsers);
    }

    /// <summary>
    /// Fetch all users with defined roles in Keycloak.
    /// Add them or update the database with these users.
    /// </summary>
    /// <param name="users"></param>
    /// <returns></returns>
    private async Task SyncKeycloakUsersWithLocal(IEnumerable<User> users)
    {
        // Fetch all users from keycloak that have one of the defined roles.
        var roles = await _cssService.GetRolesAsync();
        var cssUsers = new List<UserModel>();
        foreach (var role in roles)
        {
            var roleModels = await _cssService.GetUsersForRoleAsync(role.Name);
            cssUsers.AddRange(roleModels.Users);
        }
        cssUsers = cssUsers.DistinctBy(u => u.Username).ToList();

        // For each user in keycloak, add it to the local database.
        foreach (var cssUser in cssUsers)
        {
            // If the user has a matching key we assume that it has already been synced.
            var dbUser = users.FirstOrDefault(u => u.Key == cssUser.Username);
            if (dbUser == null)
            {
                // Extract friendly username.  There is no guarantee that the username attribute is available until the user logs in the first time.
                // This is regrettable, but for now we'll ignore the user and move on.
                var username = cssUser.Attributes.GetUsername();
                if (!String.IsNullOrWhiteSpace(username))
                {
                    dbUser = users.FirstOrDefault(u => String.Equals(u.Username, username, StringComparison.InvariantCultureIgnoreCase));
                    await AddOrUpdateUserAsync(username, dbUser, cssUser);
                }
            }
        }
    }

    /// <summary>
    /// Update the user in the database with the specified keycloak user information.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="user"></param>
    /// <param name="cssUser"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    private async Task AddOrUpdateUserAsync(string username, Entities.User? user, TNO.CSS.Models.UserModel cssUser)
    {
        user ??= new Entities.User(username, cssUser.Email ?? "", cssUser.Username);

        // user.Key = kUser.Id;
        user.Username = username;
        user.Key = cssUser.Username;
        user.Email = cssUser.Email ?? user.Email;
        user.FirstName = cssUser.FirstName ?? user.FirstName;
        user.LastName = cssUser.LastName ?? user.LastName;
        user.EmailVerified = true;
        user.IsEnabled = true;
        user.DisplayName = cssUser.Attributes["display_name"]?.FirstOrDefault() ?? user.DisplayName;

        // Fetch the roles for the user
        var userRoles = await _cssService.GetRolesForUserAsync(cssUser.Username);
        user.Roles = String.Join(",", userRoles.Roles.Select(r => $"[{r.Name}]"));

        if (user.Id == 0)
            _userService.AddAndSave(user);
        else
            _userService.UpdateAndSave(user);
    }

    /// <summary>
    /// Activate the user with TNO and CSS.
    /// If the user doesn't currently exist in TNO, activate a new user by adding them to TNO.
    /// If the user exists in TNO, activate user by linking to CSS and updating CSS.
    /// </summary>
    /// <param name="principal"></param>
    /// <returns></returns>
    public async Task<Entities.User?> ActivateAsync(ClaimsPrincipal principal)
    {
        // CSS uses the preferred_username value as a username, but it's not the actual username...
        var key = principal.GetKey() ?? throw new NotAuthorizedException("The 'preferred_username' is required but missing from token");
        var user = _userService.FindByUserKey(key);

        // If user doesn't exist, add them to the database.
        if (user == null)
        {
            var username = principal.GetUsername() ?? throw new NotAuthorizedException("The 'username' is required by missing from token");
            var email = principal.GetEmail() ?? "";
            user = _userService.FindByUsername(username);

            if (user == null && !String.IsNullOrWhiteSpace(email))
            {
                // Check if the user has been manually added by their email address.
                var users = _userService.FindByEmail(email).Where(u => u.IsEnabled);

                // If only one account has the email, we can assume it's a preapproved user.
                if (users.Count() == 1) user = users.First();
                else if (users.Count() > 1) throw new NotAuthorizedException($"There are multiple enabled users with the same email '{email}'");
            }

            // Fetch the roles for the user
            var userRoles = await _cssService.GetRolesForUserAsync(key);
            if (userRoles.Users.Length > 1) throw new NotAuthorizedException($"Keycloak has multiple users with the same username '{key}'");
            if (user == null)
            {
                // Add the user to the database.
                user = _userService.AddAndSave(new Entities.User(username, email, key)
                {
                    DisplayName = principal.GetDisplayName() ?? "",
                    FirstName = principal.GetFirstName() ?? "",
                    LastName = principal.GetLastName() ?? "",
                    IsEnabled = true,
                    EmailVerified = principal.GetEmailVerified() ?? false,
                    IsSystemAccount = false,
                    Status = Entities.UserStatus.Activated,
                    LastLoginOn = DateTime.UtcNow,
                    Roles = String.Join(",", userRoles.Roles.Select(r => $"[{r.Name}]"))
                });
            }
            else if (user != null)
            {
                // The user was created in TNO initially, but now the user has logged in and activated their account.
                user.Username = username;
                user.DisplayName = principal.GetDisplayName() ?? user.DisplayName;
                user.Key = key;
                user.Email = !String.IsNullOrWhiteSpace(email) ? email : user.Email;
                user.FirstName = principal.GetFirstName() ?? user.FirstName;
                user.LastName = principal.GetLastName() ?? user.LastName;
                user.LastLoginOn = DateTime.UtcNow;
                user.Status = Entities.UserStatus.Approved;
                user.EmailVerified = principal.GetEmailVerified() ?? false;

                // Apply the preapproved roles to the user.
                var roles = await UpdateUserRolesAsync(key, user.Roles.Split(",").Select(r => r[1..^1]).ToArray());
                user.Roles = String.Join(",", roles.Select(r => $"[{r}]"));
                _userService.UpdateAndSave(user);
                return user;
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
    /// Update the specified user with the specified roles.
    /// Only add the roles that exist in Keycloak.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    public async Task<string[]> UpdateUserRolesAsync(string username, string[] roles)
    {
        var userRoles = await _cssService.GetRolesForUserAsync(username);
        if (userRoles.Users.Length == 0) userRoles = new UserRoleResponseModel() { };
        else if (userRoles.Users.Length > 1) throw new InvalidOperationException($"There is more than one user with this username '{username}'");

        // Only update roles that exist in keycloak.
        var allRoles = await _cssService.GetRolesAsync();
        var updateRoles = allRoles?.Where(r => roles.Contains(r.Name))?.ToArray() ?? Array.Empty<RoleModel>();

        // Remove roles that the user currently has but are not in the updated array.
        var removeRoles = userRoles.Roles.Where(r => !updateRoles.Any(ur => ur.Name == r.Name)).ToArray();
        await removeRoles.ForEachAsync(async r => await _cssService.DeleteUserRoleAsync(username, r.Name));

        // Add new roles added to the user.
        var addRoles = updateRoles.Where(ur => !userRoles.Roles.Any(rr => rr.Name == ur.Name)).ToArray();
        await addRoles.ForEachAsync(async r => await _cssService.AddUserRoleAsync(username, r.Name));

        var result = await _cssService.GetRolesForUserAsync(username);
        return result.Roles.Select(r => r.Name).ToArray();
    }

    /// <summary>
    /// Remove all roles from the user in keycloak and delete the user from the local database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task DeleteUserAsync(Entities.User model)
    {
        var userRoles = await _cssService.GetRolesForUserAsync(model.Key);
        await userRoles.Roles.ForEachAsync(async r => await _cssService.DeleteUserRoleAsync(model.Key, r.Name));
        _userService.DeleteAndSave((Entities.User)model);
    }
    #endregion
}
