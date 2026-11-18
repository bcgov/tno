using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TNO.CSS.Models;

namespace TNO.CSS;

public interface ICssService
{
    #region Methods
    #region Integrations
    Task<IEnumerable<IntegrationModel>> GetIntegrationsAsync();

    Task<IntegrationModel?> GetIntegrationAsync(int id);
    #endregion

    #region Roles
    Task<IEnumerable<RoleModel>> GetRolesAsync(int integrationId, string environment);

    Task<RoleModel> AddRoleAsync(int integrationId, string environment, string name);

    Task<RoleModel?> GetRoleAsync(int integrationId, string environment, string name);

    Task<RoleModel?> UpdateRoleAsync(int integrationId, string environment, string name, string newName);

    Task DeleteRoleAsync(int integrationId, string environment, string name);
    #endregion

    #region Composite Roles
    Task<IEnumerable<RoleModel>> GetCompositeRolesAsync(int integrationId, string environment, string name);

    Task<RoleModel> AddCompositeRoleAsync(int integrationId, string environment, string name, IEnumerable<string> roles);

    Task<RoleModel?> GetCompositeRoleAsync(int integrationId, string environment, string name, string compositeName);

    Task DeleteCompositeRoleAsync(int integrationId, string environment, string name, string compositeName);
    #endregion

    #region Role Mapping
    Task<UserRoleResponseModel> GetRolesForUserAsync(int integrationId, string environment, string username);

    Task<UserRoleResponseModel> GetUsersForRoleAsync(int integrationId, string environment, string role);

    Task<UserRoleResponseModel?> AddUserRoleAsync(int integrationId, string environment, string username, string role);

    Task DeleteUserRoleAsync(int integrationId, string environment, string username, string role);
    #endregion

    #region Users
    Task<IEnumerable<UserModel>> GetUsersAsync(string environment, UserFilter filter);
    #endregion
    #endregion
}
