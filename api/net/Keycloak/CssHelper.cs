using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.CSS;
using TNO.CSS.Extensions;
using TNO.CSS.Models;
using TNO.DAL.Services;

namespace TNO.API.CSS;

/// <summary>
/// CSSHelper class, provides helper methods to manage and sync CSS.
/// </summary>
public class CssHelper : ICssHelper
{
    #region Variables
    private readonly ICssEnvironmentService _cssService;
    private readonly IUserService _userService;
    private readonly ILogger<CssHelper> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CSSHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="cssService"></param>
    /// <param name="userService"></param>
    /// <param name="logger"></param>
    public CssHelper(ICssEnvironmentService cssService, IUserService userService, ILogger<CssHelper> logger)
    {
        _cssService = cssService;
        _userService = userService;
        _logger = logger;
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
    private async Task SyncKeycloakUsersWithLocal(IEnumerable<Entities.User> users)
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
    /// <param name="location"></param>
    /// <returns></returns>
    public async Task<Tuple<Entities.User?, Models.Auth.AccountAuthState>> ActivateAsync(ClaimsPrincipal principal, Models.Auth.LocationModel? location = null)
    {
        // CSS uses the preferred_username value as a username, but it's not the actual username...
        var key = principal.GetUid() ?? throw new NotAuthorizedException("The 'sub' is required but missing from token");
        var user = _userService.FindByUserKey(key);
        var auth = Models.Auth.AccountAuthState.Authorized;

        // If user doesn't exist, add them to the database.
        if (user == null)
        {
            _logger.LogTrace("Could not find an existing user using key:[{key}]", key);
            var username = principal.GetUsername() ?? throw new NotAuthorizedException("The 'username' is required by missing from token");
            var email = principal.GetEmail() ?? "";
            user = _userService.FindByUsername(username);

            if (user == null && !String.IsNullOrWhiteSpace(email))
            {
                _logger.LogTrace("Could not find an existing user using username:[{username}], trying by email:[{email}]", username, email);
                // Check if the user has been manually added by their email address.
                var users = _userService.FindByEmail(email).Where(u => u.IsEnabled && (u.Status == Entities.UserStatus.Preapproved || u.Status == Entities.UserStatus.Approved));

                // If only one account has the email, we can assume it's a preapproved user.
                if (users.Count() == 1) user = users.First();
                else if (users.Count() > 1) throw new NotAuthorizedException($"There are multiple enabled users with the same email '{email}'");
            }

            // Fetch the roles for the user
            var userRoles = await _cssService.GetRolesForUserAsync(key);
            if (userRoles.Users.Length > 1) throw new NotAuthorizedException($"Keycloak has multiple users with the same username '{key}'");
            if (user == null)
            {
                _logger.LogTrace("Need to create a new MMI user with username:[{username}]", username);

                user = new Entities.User(username, email, key)
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
                };
                // Add the user to the database.
                user = _userService.AddAndSave(user);
            }
            else if (user != null)
            {
                _logger.LogTrace("Found existing user with username:[{username}]. Update Status and Roles", username);

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
                var roles = await UpdateUserRolesAsync(key, user.Roles.Replace("[", "").Replace("]", "").Split(",").Select(r => r.Trim()).ToArray());
                user.Roles = String.Join(",", roles.Select(r => $"[{r}]"));
                user = _userService.UpdateAndSave(user);
            }
        }
        else
        {
            _logger.LogTrace("Found existing user with key:[{key}], check Roles in CSS vs DB", key);
            var userRoles = await _cssService.GetRolesForUserAsync(key);
            var rolesInCss = userRoles.Roles.Select(r => r.Name).Order();
            var rolesInDb = user.Roles.Replace("[", "").Replace("]", "").Split(",").Select(r => r.Trim()).Order();
            if (!rolesInCss.SequenceEqual(rolesInDb))
            {
                _logger.LogTrace("User with key:[{key}] has mis-matched roles. CSS:[{rolesInCss}] DB:[{rolesInDb}]. CSS will be updated.", key, String.Join(",", rolesInCss), String.Join(",", rolesInDb));
                await UpdateUserRolesAsync(key, rolesInDb.ToArray());
            }

            user.LastLoginOn = DateTime.UtcNow;
            user = _userService.UpdateAndSave(user);
        }

        if (user != null) auth = AuthorizeLocation(user, location);

        return Tuple.Create(user, auth);
    }

    /// <summary>
    /// Remove the specified location.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="deviceKey"></param>
    public void RemoveLocation(ClaimsPrincipal principal, string? deviceKey = null)
    {
        // CSS uses the preferred_username value as a username, but it's not the actual username...
        var key = principal.GetUid() ?? throw new NotAuthorizedException("The 'sub' is required but missing from token");
        var user = _userService.FindByUserKey(key);
        if (user != null && deviceKey != null)
        {
            Models.Auth.LocationModel[]? userLocations = null;
            var locations = user.Preferences.GetElementValue<Models.Auth.LocationModel[]>(".locations");
            if (locations != null)
            {
                userLocations = locations.Where(l => l.Key != deviceKey).ToArray();
                var preferences = JsonNode.Parse(user.Preferences.ToJson())?.AsObject();
                if (preferences != null)
                {
                    preferences.Remove("locations");
                    var arrayNode = new JsonArray();
                    userLocations.ForEach(location => arrayNode.Add(location));
                    preferences.Add("locations", arrayNode);
                    user.Preferences = JsonDocument.Parse(preferences.ToJsonString());
                    _userService.UpdateAndSave(user);
                }
            }
        }
    }

    /// <summary>
    /// Keep the specified location and remove all others.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="deviceKey"></param>
    public void RemoveOtherLocations(ClaimsPrincipal principal, string? deviceKey = null)
    {
        // CSS uses the preferred_username value as a username, but it's not the actual username...
        var key = principal.GetUid() ?? throw new NotAuthorizedException("The 'sub' is required but missing from token");
        var user = _userService.FindByUserKey(key);
        if (user != null && deviceKey != null)
        {
            Models.Auth.LocationModel[]? userLocations = null;
            var locations = user.Preferences.GetElementValue<Models.Auth.LocationModel[]>(".locations");
            if (locations != null)
            {
                userLocations = locations.Where(l => l.Key == deviceKey).ToArray();
                var preferences = JsonNode.Parse(user.Preferences.ToJson())?.AsObject();
                if (preferences != null)
                {
                    preferences.Remove("locations");
                    var arrayNode = new JsonArray();
                    userLocations.ForEach(location => arrayNode.Add(location));
                    preferences.Add("locations", arrayNode);
                    user.Preferences = JsonDocument.Parse(preferences.ToJsonString());
                    _userService.UpdateAndSave(user);
                }
            }
        }
    }

    /// <summary>
    /// If a location is provided it will add it to the user preferences property.
    /// Determine if the user has logged into too many devices at the same time.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="location"></param>
    /// <returns></returns>
    private static Models.Auth.AccountAuthState AuthorizeLocation(Entities.User user, Models.Auth.LocationModel? location = null)
    {
        var state = Models.Auth.AccountAuthState.Authorized;
        if (location != null)
        {
            location.LastLoginOn = DateTime.UtcNow;
            var reasonableTimeSpan = location.LastLoginOn.Value.AddMinutes(-5);
            var locations = user.Preferences.GetElementValue<Models.Auth.LocationModel[]>(".locations");
            Models.Auth.LocationModel[]? userLocations = null;
            if (locations == null)
            {
                // The first location captured.
                userLocations = new[] { location };
            }
            else if (!locations.Any(l => l?.IPv4 == location?.IPv4) == true)
            {
                // Place new location at the top of the array.
                userLocations = new Models.Auth.LocationModel[locations.Length + 1];
                userLocations[0] = location;
                Array.Copy(locations, 0, userLocations, 1, locations.Length);

                // Multiple IP address may indicate a possible shared account if the login timestamps are close.
                if (userLocations.Count(l => l.LastLoginOn >= reasonableTimeSpan) > 1)
                {
                    // TODO: It's possible the same device has moved to a new IP.
                    // Inform action on shared account over multiple locations.
                    state = Models.Auth.AccountAuthState.MultipleIPs;
                }
            }
            else
            {
                // This IP has already been captured, it needs to be updated with the latest information.
                // Multiple devices with the same IP may indicate a possible shared account if the key are different.
                // It could also simply mean the same account logged into multiple devices.
                var add = true;
                for (var i = 0; i < locations.Length; i++)
                {
                    if (locations[i].IPv4 == location.IPv4)
                    {
                        if (locations[i].Key == location.Key)
                        {
                            locations[i].LastLoginOn = location.LastLoginOn;
                            add = false;
                        }
                        else if (location.LastLoginOn >= reasonableTimeSpan)
                        {
                            // A different key may indicate different devices.
                            // Each browser will generate a unique key.
                            // Keys can get reset when cache is cleared.
                            state = Models.Auth.AccountAuthState.MultipleDevices;
                        }
                    }
                }
                if (add)
                {
                    // Place new device at the top of the array.
                    userLocations = new Models.Auth.LocationModel[locations.Length + 1];
                    userLocations[0] = location;
                    Array.Copy(locations, 0, userLocations, 1, locations.Length);
                }
                else
                {
                    userLocations = locations;
                }
            }

            if (user.UniqueLogins == 0) state = Models.Auth.AccountAuthState.Authorized;
            if ((state == Models.Auth.AccountAuthState.MultipleIPs && userLocations.Length > user.UniqueLogins) ||
                (state == Models.Auth.AccountAuthState.MultipleDevices && userLocations.Length > user.UniqueLogins)) state = Models.Auth.AccountAuthState.Unauthorized;

            // Update user preferences with location data.
            var preferences = JsonNode.Parse(user.Preferences.ToJson())?.AsObject();
            if (preferences != null)
            {
                // Remove older locations otherwise when a user attempts to login with a new device key it will always fail.
                var length = user.UniqueLogins > 0 ? user.UniqueLogins : userLocations.Length;
                userLocations = userLocations.Where(l => l.LastLoginOn >= location.LastLoginOn.Value.AddMonths(-1)).Take(length).ToArray();

                preferences.Remove("locations");
                var arrayNode = new JsonArray();
                userLocations.ForEach(location => arrayNode.Add(location));
                preferences.Add("locations", arrayNode);
                user.Preferences = JsonDocument.Parse(preferences.ToJsonString());
            }

            return state;
        }

        return state;
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
