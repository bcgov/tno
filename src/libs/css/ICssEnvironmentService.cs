using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TNO.CSS.Config;
using TNO.CSS.Models;

namespace TNO.CSS;

public interface ICssEnvironmentService
{
    #region Properties
    CssEnvironmentOptions Options { get; }
    #endregion

    #region Methods
    #region Integrations
    Task<IEnumerable<IntegrationModel>> GetIntegrationsAsync();

    Task<IntegrationModel?> GetIntegrationAsync();
    #endregion

    #region Roles
    Task<IEnumerable<RoleModel>> GetRolesAsync();

    Task<RoleModel> AddRoleAsync(string name);

    Task<RoleModel?> GetRoleAsync(string name);

    Task<RoleModel?> UpdateRoleAsync(string name, string newName);

    Task DeleteRoleAsync(string name);
    #endregion

    #region Composite Roles
    Task<IEnumerable<RoleModel>> GetCompositeRolesAsync(string name);

    Task<RoleModel> AddCompositeRoleAsync(string name, IEnumerable<string> roles);

    Task<RoleModel?> GetCompositeRoleAsync(string name, string compositeName);

    Task DeleteCompositeRoleAsync(string name, string compositeName);
    #endregion

    #region Role Mapping
    Task<UserRoleResponseModel> GetRolesForUserAsync(string username);

    Task<UserRoleResponseModel> GetUsersForRoleAsync(string role);

    Task<UserRoleResponseModel?> AddUserRoleAsync(string username, string role);

    Task DeleteUserRoleAsync(string username, string role);
    #endregion

    #region Users
    Task<IEnumerable<UserModel>> GetUsersAsync(UserFilter filter);
    #endregion
    #endregion
}
