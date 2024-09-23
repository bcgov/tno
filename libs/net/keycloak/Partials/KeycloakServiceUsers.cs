using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TNO.Core.Extensions;
using TNO.Keycloak.Extensions;
using TNO.Keycloak.Models;

namespace TNO.Keycloak
{
    /// <summary>
    /// KeycloakAdmin class, provides a service for sending HTTP requests to the keycloak admin API.
    ///     - https://www.keycloak.org/docs-api/5.0/rest-api/index.html#_overview
    /// </summary>
    public partial class KeycloakService : IKeycloakService
    {
        #region Methods
        /// <summary>
        /// Get the total number of users.
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetUserCountAsync()
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/users/count");
            var result = await response.HandleResponseAsync<Models.CountModel>();

            return result?.Count ?? 0;
        }

        /// <summary>
        /// Get an array of users.
        /// This function supports paging.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="max"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public async Task<Models.UserModel[]> GetUsersAsync(int first = 0, int max = 10, UserFilter? filter = null)
        {
            var uri = new UriBuilder($"{GetBaseUrl()}/users?first={first}&max={max}");
            var query = HttpUtility.ParseQueryString(uri.Query);
            if (!String.IsNullOrWhiteSpace(filter?.Search)) query.Add("search", filter.Search);
            if (!String.IsNullOrWhiteSpace(filter?.Username)) query.Add("username", filter.Username);
            if (!String.IsNullOrWhiteSpace(filter?.Email)) query.Add("email", filter.Email);
            if (!String.IsNullOrWhiteSpace(filter?.FirstName)) query.Add("firstName", filter.FirstName);
            if (!String.IsNullOrWhiteSpace(filter?.LastName)) query.Add("lastName", filter.LastName);
            if (filter?.Enabled.HasValue == true) query.Add("enabled", filter.Enabled?.ToString());
            if (filter?.Exact.HasValue == true) query.Add("exact", filter.Exact?.ToString());
            if (filter?.EmailVerified.HasValue == true) query.Add("emailVerified", filter.EmailVerified?.ToString());
            uri.Query = query.ToString();
            var response = await _client.GetAsync(uri.Uri);

            return await response.HandleResponseAsync<Models.UserModel[]>() ?? Array.Empty<Models.UserModel>();
        }

        /// <summary>
        /// Get the user for the specified 'id'.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Models.UserModel?> GetUserAsync(Guid id)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/users/{id}");

            return await response.HandleResponseAsync<Models.UserModel>();
        }

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Models.UserModel?> CreateUserAsync(Models.UserModel user)
        {
            var json = user.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PostAsync($"{GetBaseUrl()}/users", content);

            return await response.HandleResponseAsync<Models.UserModel>();
        }

        /// <summary>
        /// Update the specified user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<Guid> UpdateUserAsync(Models.UserModel user)
        {
            var json = user.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PutAsync($"{GetBaseUrl()}/users/{user.Id}", content);

            return response.HandleResponse(user.Id);
        }

        /// <summary>
        /// Delete the user for the specified 'id'.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Guid> DeleteUserAsync(Guid id)
        {
            var response = await _client.DeleteAsync($"{GetBaseUrl()}/users/{id}");

            return response.HandleResponse(id);
        }

        /// <summary>
        /// Get an array of the groups the user for the specified 'id' is a member of.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Models.GroupModel[]> GetUserGroupsAsync(Guid id)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/users/{id}/groups");

            return await response.HandleResponseAsync<Models.GroupModel[]>() ?? Array.Empty<Models.GroupModel>();
        }

        /// <summary>
        /// Get the total number of groups the user for the specified 'id' is a member of.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> GetUserGroupCountAsync(Guid id)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/users/{id}/groups/count");

            return await response.HandleResponseAsync<int>();
        }

        /// <summary>
        /// Add the user to the group for the specified 'userId' and 'groupId'.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<Guid> AddGroupToUserAsync(Guid userId, Guid groupId)
        {
            var response = await _client.PutAsync($"{GetBaseUrl()}/users/{userId}/groups/{groupId}");

            return response.HandleResponse(userId);
        }

        /// <summary>
        /// Remove the user from the group for the specified 'userId' and 'groupId'.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public async Task<Guid> RemoveGroupFromUserAsync(Guid userId, Guid groupId)
        {
            var response = await _client.DeleteAsync($"{GetBaseUrl()}/users/{userId}/groups/{groupId}");

            return response.HandleResponse(userId);
        }

        #region Client Roles
        /// <summary>
        /// Get the client roles for the specified 'userId'.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel[]> GetUserClientRolesAsync(Guid userId, Guid clientId)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/users/{userId}/role-mappings/clients/{clientId}");

            return await response.HandleResponseAsync<Models.RoleModel[]>() ?? Array.Empty<Models.RoleModel>();
        }

        /// <summary>
        /// Add the user to the client role for the specified 'userId' and 'clientId'.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="clientId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task<Guid> AddUserClientRolesAsync(Guid userId, Guid clientId, Models.RoleModel[] roles)
        {
            var json = roles.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PostAsync($"{GetBaseUrl()}/users/{userId}/role-mappings/clients/{clientId}", content);

            return response.HandleResponse(userId);
        }

        /// <summary>
        /// Remove the user from the group for the specified 'userId' and 'groupId'.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="clientId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task<Guid> RemoveUserClientRolesAsync(Guid userId, Guid clientId, Models.RoleModel[] roles)
        {
            var json = roles.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.DeleteAsync($"{GetBaseUrl()}/users/{userId}/role-mappings/clients/{clientId}", content);

            return response.HandleResponse(userId);
        }
        #endregion
        #endregion
    }
}
