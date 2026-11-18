using TNO.Core.Extensions;
using TNO.Keycloak.Extensions;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mime;

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
        /// Get an array of roles for the client specified by the 'clientId'.
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel[]> GetRolesAsync(Guid clientId)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/clients/{clientId}/roles");

            return await response.HandleResponseAsync<Models.RoleModel[]>() ?? Array.Empty<Models.RoleModel>();
        }

        /// <summary>
        /// Get the role specified by the 'name', within the client specified by the 'clientId'
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel?> GetRoleAsync(Guid clientId, string name)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/clients/{clientId}/roles/{name}");

            return await response.HandleResponseAsync<Models.RoleModel>();
        }

        /// <summary>
        /// Create a new role within the client specified by the 'clientId'.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel?> CreateRoleAsync(Guid clientId, Models.RoleModel role)
        {
            var json = role.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PostAsync($"{GetBaseUrl()}/clients/{clientId}/roles", content);

            return await response.HandleResponseAsync<Models.RoleModel>();
        }

        /// <summary>
        /// Update the role within the client specified by the 'clientId'.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel?> UpdateRoleAsync(Guid clientId, Models.RoleModel role)
        {
            var json = role.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PutAsync($"{GetBaseUrl()}/clients/{clientId}/roles/{role.Name}", content);

            return response.HandleResponse(role);
        }

        /// <summary>
        /// Delete the role specified by the 'name' within the client specified by the 'clientId'.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<string> DeleteRoleAsync(Guid clientId, string name)
        {
            var response = await _client.DeleteAsync($"{GetBaseUrl()}/clients/{clientId}/roles/{name}");

            return response.HandleResponse(name);
        }

        /// <summary>
        /// Create a new composite role within the client specified by the 'clientId', the belongs to the role specified by the 'parentName'.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="parentName"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel?> CreateCompositeRoleAsync(Guid clientId, string parentName, Models.RoleModel role)
        {
            var json = role.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.PostAsync($"{GetBaseUrl()}/clients/{clientId}/roles/{parentName}/composites", content);

            return await response.HandleResponseAsync<Models.RoleModel>();
        }

        /// <summary>
        /// Get an array of composite roles
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel[]> GetCompositeRolesAsync(Guid clientId, string parentName)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/clients/{clientId}/roles/{parentName}/composites");

            return await response.HandleResponseAsync<Models.RoleModel[]>() ?? Array.Empty<Models.RoleModel>();
        }

        /// <summary>
        /// Delete composite roles within the client specified by the 'clientId', that belong to the parent role specified by the 'parentName'.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="parentName"></param>
        /// <param name="roles"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel[]> DeleteCompositeRoleAsync(Guid clientId, string parentName, Models.RoleModel[] roles)
        {
            var json = roles.Serialize();
            var content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await _client.DeleteAsync($"{GetBaseUrl()}/clients/{clientId}/roles/{parentName}/composites", content);

            return response.HandleResponse(roles);
        }

        /// <summary>
        /// Get an array of composite roles within the client specified by the 'clientId', that belong to the parent role specified by the 'parentName'
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="parentName"></param>
        /// <param name="clientName"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel[]> GetClientCompositeRolesAsync(Guid clientId, string parentName, string clientName)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/clients/{clientId}/roles/{parentName}/composites/clients/{clientName}");

            return await response.HandleResponseAsync<Models.RoleModel[]>() ?? Array.Empty<Models.RoleModel>();
        }

        /// <summary>
        /// Get an array of composite roles within the client specified by the 'clientId', that belong to the parent role specified by the 'parentName'
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public async Task<Models.RoleModel[]> GetRealmCompositeRolesAsync(Guid clientId, string parentName)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/clients/{clientId}/roles/{parentName}/composites/realm");

            return await response.HandleResponseAsync<Models.RoleModel[]>() ?? Array.Empty<Models.RoleModel>();
        }

        /// <summary>
        /// Get an array of users who are within the client specified by the 'clientId', that belong to the parent role specified by the 'parentName'.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="parentName"></param>
        /// <param name="first"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public async Task<Models.UserModel[]> GetRoleMembersAsync(Guid clientId, string parentName, int first = 0, int max = 10)
        {
            var response = await _client.GetAsync($"{GetBaseUrl()}/clients/{clientId}/roles/{parentName}/users?first={first}&max={max}");

            return await response.HandleResponseAsync<Models.UserModel[]>() ?? Array.Empty<Models.UserModel>();
        }
        #endregion
    }
}
