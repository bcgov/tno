using System;
using System.Threading.Tasks;
using TNO.Core.Http.Models;

namespace TNO.Keycloak;

/// <summary>
/// KeycloakService class, provides a service for sending HTTP requests to the keycloak admin API.
///     - https://www.keycloak.org/docs-api/5.0/rest-api/index.html#_overview
/// </summary>
public interface IKeycloakService
{
    #region Token
    Task<TokenModel?> RequestTokenAsync();
    #endregion

    #region Attack Detection
    Task DeleteAttackDetectionAsync();
    #endregion

    #region Users
    Task<int> GetUserCountAsync();
    Task<Models.UserModel[]> GetUsersAsync(int first = 0, int max = 10, UserFilter? filter = null);
    Task<Models.UserModel?> GetUserAsync(Guid id);
    Task<Models.UserModel?> CreateUserAsync(Models.UserModel user);
    Task<Guid> UpdateUserAsync(Models.UserModel user);
    Task<Guid> DeleteUserAsync(Guid id);
    Task<Models.GroupModel[]> GetUserGroupsAsync(Guid id);
    Task<int> GetUserGroupCountAsync(Guid id);
    Task<Guid> AddGroupToUserAsync(Guid userId, Guid groupId);
    Task<Guid> RemoveGroupFromUserAsync(Guid userId, Guid groupId);
    Task<Models.RoleModel[]> GetUserClientRolesAsync(Guid userId, Guid clientId);
    Task<Guid> AddUserClientRolesAsync(Guid userId, Guid clientId, Models.RoleModel[] roles);
    Task<Guid> RemoveUserClientRolesAsync(Guid userId, Guid clientId, Models.RoleModel[] roles);
    #endregion

    #region Groups
    Task<int> GetGroupCountAsync();
    Task<Models.GroupModel[]> GetGroupsAsync(int first = 0, int max = 10, string? search = null);
    Task<Models.GroupModel?> GetGroupAsync(Guid id);
    Task<Models.GroupModel?> CreateGroupAsync(Models.GroupModel group);
    Task<Models.GroupModel?> CreateSubGroupAsync(Guid parentId, Models.GroupModel group);
    Task<Models.GroupModel?> UpdateGroupAsync(Models.GroupModel group);
    Task<Guid> DeleteGroupAsync(Guid id);
    Task<Models.UserModel[]> GetGroupMembersAsync(Guid id, int first = 0, int max = 10);
    Task<Models.RoleModel[]> GetClientRolesForGroupAsync(Guid groupId, Guid clientId);
    Task AddClientRolesForGroupAsync(Guid groupId, Guid clientId, Models.RoleModel[] roles);
    Task RemoveClientRolesForGroupAsync(Guid groupId, Guid clientId, Models.RoleModel[] roles);
    #endregion

    #region Roles
    #region By ID
    Task<Models.RoleModel?> GetRoleAsync(Guid id);
    Task<Models.RoleModel?> UpdateRoleAsync(Models.RoleModel role);
    Task<Guid> DeleteRoleAsync(Guid id);
    Task<Models.RoleModel[]> CreateCompositeRolesAsync(Guid parentId, Models.RoleModel[] roles);
    Task<Models.RoleModel[]> GetCompositeRolesAsync(Guid parentId);
    Task<Models.RoleModel[]> DeleteCompositeRolesAsync(Guid parentId, Models.RoleModel[] roles);
    Task<Models.RoleModel[]> GetClientCompositeRolesAsync(Guid parentId, string clientName);
    Task<Models.RoleModel[]> GetRealmCompositeRolesAsync(Guid parentId);
    #endregion

    #region Realm
    Task<Models.RoleModel[]> GetRolesAsync();
    Task<Models.RoleModel?> GetRoleAsync(string name);
    Task<Models.RoleModel?> CreateRoleAsync(Models.RoleModel role);
    Task<Models.RoleModel?> UpdateRoleAsync(string name, Models.RoleModel role);
    Task<string> DeleteRoleAsync(string name);
    Task<Models.RoleModel?> CreateCompositeRoleAsync(string parentName, Models.RoleModel role);
    Task<Models.RoleModel[]> GetCompositeRolesAsync(string parentName);
    Task<Models.RoleModel[]> DeleteCompositeRolesAsync(string parentName, Models.RoleModel[] roles);
    Task<Models.RoleModel[]> GetClientCompositeRolesAsync(string parentName, string clientName);
    Task<Models.RoleModel[]> GetRealmCompositeRolesAsync(string parentName);
    Task<Models.UserModel[]> GetRoleMembersAsync(string parentName, int first = 0, int max = 10);
    #endregion

    #region Client
    Task<Models.RoleModel[]> GetRolesAsync(Guid clientId);
    Task<Models.RoleModel?> GetRoleAsync(Guid clientId, string name);
    Task<Models.RoleModel?> CreateRoleAsync(Guid clientId, Models.RoleModel role);
    Task<Models.RoleModel?> UpdateRoleAsync(Guid clientId, Models.RoleModel role);
    Task<string> DeleteRoleAsync(Guid clientId, string name);
    Task<Models.RoleModel?> CreateCompositeRoleAsync(Guid clientId, string parentName, Models.RoleModel role);
    Task<Models.RoleModel[]> GetCompositeRolesAsync(Guid clientId, string parentName);
    Task<Models.RoleModel[]> DeleteCompositeRoleAsync(Guid clientId, string parentName, Models.RoleModel[] roles);
    Task<Models.RoleModel[]> GetClientCompositeRolesAsync(Guid clientId, string parentName, string clientName);
    Task<Models.RoleModel[]> GetRealmCompositeRolesAsync(Guid clientId, string parentName);
    Task<Models.UserModel[]> GetRoleMembersAsync(Guid clientId, string parentName, int first = 0, int max = 10);
    #endregion
    #endregion
}

