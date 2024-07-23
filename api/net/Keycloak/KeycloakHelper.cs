using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
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
    private readonly ILogger<KeycloakHelper> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KeycloakHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="keycloakService"></param>
    /// <param name="userService"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public KeycloakHelper(
        IKeycloakService keycloakService,
        IUserService userService,
        IOptions<Config.KeycloakOptions> options,
        ILogger<KeycloakHelper> logger)
    {
        _keycloakService = keycloakService;
        _userService = userService;
        _options = options.Value;
        _logger = logger;
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
        user.DisplayName = kUser.GetDisplayName() ?? user.DisplayName;

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
    /// <param name="location"></param>
    /// <returns></returns>
    public async Task<Tuple<Entities.User?, Models.Auth.AccountAuthState>> ActivateAsync(ClaimsPrincipal principal, Models.Auth.LocationModel? location = null)
    {
        if (!_options.ClientId.HasValue) throw new ConfigurationException("Keycloak clientId has not been configured");
        var auth = Models.Auth.AccountAuthState.Authorized;

        if (!Guid.TryParse(principal.GetIdentifier(), out Guid keycloakUid)) throw new NotAuthorizedException("The 'sub' is required but missing from token");
        var user = _userService.FindByUserKey(keycloakUid.ToString());

        // Check if the account exists with the old key from the shared keycloak realm.
        // We need to do this because we're migrating over to the new custom realm.
        if (user == null)
        {
            var uid = principal.GetUid();
            user = _userService.FindByUserKey($"{uid}@idir") ?? _userService.FindByUserKey($"{uid}@bceidbasic") ?? _userService.FindByUserKey($"{uid}");
        }

        // If user doesn't exist, add them to the database.
        if (user == null)
        {
            _logger.LogTrace("Could not find an existing user using key:[{key}]", keycloakUid);

            var username = principal.GetUsername() ?? throw new InvalidOperationException("Username is required but missing from token");
            var email = principal.GetEmail() ?? throw new InvalidOperationException("Email is required but missing from token");

            // Check if the user has been manually added by their email address.
            // If only one account has the email, we can assume it's a preapproved user.
            // However if it isn't we need to see if there is a match for the username instead (which is unlikely).
            var users = _userService.FindByEmail(email);
            if (users.Count() == 1) user = users.First();
            else user = _userService.FindByUsername(username);

            // Fetch the roles for the user
            var roles = await _keycloakService.GetUserClientRolesAsync(keycloakUid, _options.ClientId.Value);

            if (user == null)
            {
                _logger.LogTrace("Need to create a new MMI user with username:[{username}]", username);
                var kUser = await _keycloakService.GetUserAsync(keycloakUid) ?? throw new InvalidOperationException("The user does not exist in keycloak");

                // Add the user to the database.
                user = _userService.AddAndSave(new Entities.User(username, email, keycloakUid.ToString())
                {
                    DisplayName = kUser.GetDisplayName() ?? principal.GetDisplayName() ?? "",
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
                _logger.LogTrace("Found existing user with username:[{username}]. Update Status and Roles", username);

                var rolesInDb = user.Roles.Replace("[", "").Replace("]", "").Split(",").Select(r => r.Trim()).Order();
                var rolesFromKeycloak = await UpdateUserRolesAsync(keycloakUid, rolesInDb.ToArray());

                // Update the user in the database and reference the keycloak uid.
                // The user was created in TNO initially, but now the user has logged in and activated their account.
                user.Key = keycloakUid.ToString();
                user.Username = username;
                user.Email = email;
                user.EmailVerified = principal.GetEmailVerified() ?? false;
                user.FirstName = principal.GetFirstName() ?? "";
                user.LastName = principal.GetLastName() ?? "";
                user.LastLoginOn = DateTime.UtcNow;
                user.Status = Entities.UserStatus.Approved;
                user.Roles = String.Join(",", rolesFromKeycloak.Select(r => $"[{r.ToLower()}]"));
                var model = await UpdateUserAsync(new UserModel(user));
                user = (Entities.User)model;
            }
        }
        else
        {
            _logger.LogTrace("Found existing user with key:[{key}], check Roles in Keycloak vs DB", keycloakUid);
            var roles = await _keycloakService.GetUserClientRolesAsync(keycloakUid, _options.ClientId.Value);
            var rolesInKeycloak = roles.Select(r => r.Name).Order();
            var rolesInDb = user.Roles.Replace("[", "").Replace("]", "").Split(",").Select(r => r.Trim()).Order();
            if (!rolesInKeycloak.SequenceEqual(rolesInDb))
            {
                _logger.LogTrace("User with key:[{key}] has mis-matched roles. Keycloak:[{rolesInCss}] DB:[{rolesInDb}]. Keycloak will be updated.", keycloakUid, String.Join(",", rolesInKeycloak), String.Join(",", rolesInDb));
                await UpdateUserRolesAsync(keycloakUid, rolesInDb.ToArray());
            }

            if (user.Key != keycloakUid.ToString())
                user.Key = keycloakUid.ToString();
            user.LastLoginOn = DateTime.UtcNow;
            _userService.UpdateAndSave(user);
        }

        if (user != null) auth = AuthorizeLocation(user, location);

        return Tuple.Create(user, auth);
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
                kUser.Attributes ??= new Dictionary<string, string[]>();
                kUser.SetDisplayName(user.DisplayName);
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

    #region Location Methods
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
    #endregion
}
