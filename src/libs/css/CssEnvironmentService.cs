using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TNO.CSS.Config;
using TNO.CSS.Models;

namespace TNO.CSS;

/// <summary>
/// CssEnvironmentService class, provides a wrapper for the CssService to simply the API calls to an integration and environment.
/// </summary>
public class CssEnvironmentService : ICssEnvironmentService
{
    #region Properties
    /// <summary>
    /// get - The configuration options.
    /// </summary>
    public CssEnvironmentOptions Options { get; private set; }

    /// <summary>
    /// get - The CSS service.
    /// </summary>
    protected ICssService Service { get; private set; }
    #endregion

    #region Constructors
    public CssEnvironmentService(ICssService service,
        IOptionsMonitor<CssEnvironmentOptions> options)
    {
        this.Service = service;
        this.Options = options.CurrentValue;
    }
    #endregion

    #region Methods
    #region Integrations
    public async Task<IEnumerable<IntegrationModel>> GetIntegrationsAsync()
    {
        return await this.Service.GetIntegrationsAsync();
    }

    public async Task<IntegrationModel?> GetIntegrationAsync()
    {
        return await this.Service.GetIntegrationAsync(this.Options.IntegrationId);
    }
    #endregion

    #region Roles
    public async Task<IEnumerable<RoleModel>> GetRolesAsync()
    {
        return await this.Service.GetRolesAsync(this.Options.IntegrationId, this.Options.Environment);
    }

    public async Task<RoleModel> AddRoleAsync(string name)
    {
        return await this.Service.AddRoleAsync(this.Options.IntegrationId, this.Options.Environment, name);
    }

    public async Task<RoleModel?> GetRoleAsync(string name)
    {
        return await this.Service.GetRoleAsync(this.Options.IntegrationId, this.Options.Environment, name);
    }

    public async Task<RoleModel?> UpdateRoleAsync(string name, string newName)
    {
        return await this.Service.UpdateRoleAsync(this.Options.IntegrationId, this.Options.Environment, name, newName);
    }

    public async Task DeleteRoleAsync(string name)
    {
        await this.Service.DeleteRoleAsync(this.Options.IntegrationId, this.Options.Environment, name);
    }

    public async Task<IEnumerable<RoleModel>> GetCompositeRolesAsync(string name)
    {
        return await this.Service.GetCompositeRolesAsync(this.Options.IntegrationId, this.Options.Environment, name);
    }

    public async Task<RoleModel> AddCompositeRoleAsync(string name, IEnumerable<string> roles)
    {
        return await this.Service.AddCompositeRoleAsync(this.Options.IntegrationId, this.Options.Environment, name, roles);
    }

    public async Task<RoleModel?> GetCompositeRoleAsync(string name, string compositeName)
    {
        return await this.Service.GetCompositeRoleAsync(this.Options.IntegrationId, this.Options.Environment, name, compositeName);
    }

    public async Task DeleteCompositeRoleAsync(string name, string compositeName)
    {
        await this.Service.DeleteCompositeRoleAsync(this.Options.IntegrationId, this.Options.Environment, name, compositeName);
    }
    #endregion

    #region Role Mapping
    public async Task<UserRoleResponseModel> GetRolesForUserAsync(string username)
    {
        return await this.Service.GetRolesForUserAsync(this.Options.IntegrationId, this.Options.Environment, username);
    }

    public async Task<UserRoleResponseModel> GetUsersForRoleAsync(string role)
    {
        return await this.Service.GetUsersForRoleAsync(this.Options.IntegrationId, this.Options.Environment, role);
    }

    public async Task<UserRoleResponseModel?> AddUserRoleAsync(string username, string role)
    {
        return await this.Service.AddUserRoleAsync(this.Options.IntegrationId, this.Options.Environment, username, role);
    }

    public async Task DeleteUserRoleAsync(string username, string role)
    {
        await this.Service.DeleteUserRoleAsync(this.Options.IntegrationId, this.Options.Environment, username, role);
    }
    #endregion

    #region Users
    public async Task<IEnumerable<UserModel>> GetUsersAsync(UserFilter filter)
    {
        return await this.Service.GetUsersAsync(this.Options.Environment, filter);
    }
    #endregion
    #endregion
}
