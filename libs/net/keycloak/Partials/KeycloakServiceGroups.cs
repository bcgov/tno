using TNO.Core.Extensions;
using TNO.Keycloak.Extensions;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mime;
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
        /// Get the total number of groups.
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetGroupCountAsync()
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/groups/count");
            var result = await response.HandleResponseAsync<Models.CountModel>();

            return result?.Count ?? 0;
        }

        /// <summary>
        /// Get an array of groups.
        /// This method supports paging.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="max"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public async Task<Models.GroupModel[]> GetGroupsAsync(int first = 0, int max = 10, string? search = null)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/groups?first={first}&max={max}&search={search}");

            return await response.HandleResponseAsync<Models.GroupModel[]>() ?? Array.Empty<Models.GroupModel>();
        }

        /// <summary>
        /// Get the group for the specified 'id'.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Models.GroupModel?> GetGroupAsync(Guid id)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/groups/{id}");

            return await response.HandleResponseAsync<Models.GroupModel>();
        }

        /// <summary>
        /// Create a new group.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public async Task<Models.GroupModel?> CreateGroupAsync(Models.GroupModel group)
        {
            var json = group.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PostAsync($"{GetBaseUrl()}/groups", content);

            return await response.HandleResponseAsync<Models.GroupModel>();
        }

        /// <summary>
        /// Create a sub-group to the parent group specified for the 'parentId'.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public async Task<Models.GroupModel?> CreateSubGroupAsync(Guid parentId, Models.GroupModel group)
        {
            var json = group.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PostAsync($"{GetBaseUrl()}/groups/{parentId}/children", content);

            return await response.HandleResponseAsync<Models.GroupModel>();
        }

        /// <summary>
        /// Update the specified group.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public async Task<Models.GroupModel?> UpdateGroupAsync(Models.GroupModel group)
        {
            var json = group.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PutAsync($"{GetBaseUrl()}/groups/{group.Id}", content);

            return response.HandleResponse(group);
        }

        /// <summary>
        /// Delete the group specified for the 'id'.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Guid> DeleteGroupAsync(Guid id)
        {
            var response = await _client.DeleteAsync($"{GetBaseUrl()}/groups/{id}");

            return response.HandleResponse(id);
        }

        /// <summary>
        /// Get an array of users that are members of the group specified by the 'id'.
        /// This method supports paging.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="first"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public async Task<Models.UserModel[]> GetGroupMembersAsync(Guid id, int first = 0, int max = 10)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/groups/{id}/members?first={first}&max={max}");

            return await response.HandleResponseAsync<Models.UserModel[]>() ?? Array.Empty<Models.UserModel>();
        }

        /// <summary>
        /// Get an array of client roles for the specified 'groupId' and 'clientId'.
        /// This method supports paging.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel[]> GetClientRolesForGroupAsync(Guid groupId, Guid clientId)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/groups/{groupId}/role-mappings/clients/{clientId}");

            return await response.HandleResponseAsync<Models.RoleModel[]>() ?? Array.Empty<Models.RoleModel>();
        }

        /// <summary>
        /// Get an array of client roles that are members of the group specified by the 'groupId'.
        /// This method supports paging.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="clientId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task AddClientRolesForGroupAsync(Guid groupId, Guid clientId, RoleModel[] roles)
        {
            var json = roles.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PostAsync($"{GetBaseUrl()}/groups/{groupId}/role-mappings/clients/{clientId}", content);

            response.HandleResponse();
        }

        /// <summary>
        /// Get an array of client roles that are members of the group specified by the 'groupId'.
        /// This method supports paging.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="clientId"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task RemoveClientRolesForGroupAsync(Guid groupId, Guid clientId, RoleModel[] roles)
        {
            var json = roles.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.DeleteAsync($"{GetBaseUrl()}/groups/{groupId}/role-mappings/clients/{clientId}", content);

            response.HandleResponse();
        }
        #endregion
    }
}
