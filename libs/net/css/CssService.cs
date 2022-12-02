using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using TNO.Core.Extensions;
using TNO.CSS.Extensions;
using TNO.CSS.Models;
using System.Web;

namespace TNO.CSS;

/// <summary>
/// CssService class, provides a wrapper service for the CSS API.
/// </summary>
public class CssService : ICssService
{
    #region Variables
    private const string BASE_URL = "/api/v1";
    #endregion

    #region Properties
    /// <summary>
    /// get - The HTTP client to communicate with the CSS API
    /// </summary>
    public ICssClient Client { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CssService, initializes with specified parameters.
    /// </summary>
    /// <param name="client"></param>
    public CssService(ICssClient client)
    {
        this.Client = client;
    }
    #endregion

    #region Methods
    #region Helpers
    /// <summary>
    /// Create a JSON StringContent body object by serializing the specified 'item'.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    private static StringContent CreateBody<T>(T item)
    {
        return new StringContent(item.Serialize(), Encoding.UTF8, MediaTypeNames.Application.Json);
    }
    #endregion

    #region Integrations
    /// <summary>
    /// Get an array of integrations.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<IntegrationModel>> GetIntegrationsAsync()
    {
        var response = await this.Client.GetAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations"));
        var result = await response.HandleResponseAsync<DataResponseModel<IntegrationModel>>();
        return result.Data;
    }

    /// <summary>
    /// Get the integration for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IntegrationModel?> GetIntegrationAsync(int id)
    {
        var response = await this.Client.GetAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{id}"));
        return await response.HandleResponseAsync<IntegrationModel>();
    }
    #endregion

    #region Roles
    /// <summary>
    /// Get all the roles for the specified 'integrationId' and 'environment'.
    /// </summary>
    /// <param name="integrationId"></param>
    /// <param name="environment"></param>
    /// <returns></returns>
    public async Task<IEnumerable<RoleModel>> GetRolesAsync(int integrationId, string environment)
    {
        var response = await this.Client.GetAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/roles"));
        var result = await response.HandleResponseAsync<DataResponseModel<RoleModel>>();
        return result.Data;
    }

    /// <summary>
    /// Add a new role to the specified 'integrationId' and 'environment'.
    /// </summary>
    /// <param name="integrationId"></param>
    /// <param name="environment"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<RoleModel> AddRoleAsync(int integrationId, string environment, string name)
    {
        var role = new RoleModel(name);
        var response = await this.Client.PostAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/roles"), CreateBody(role));
        return await response.HandleResponseAsync<RoleModel>();
    }

    /// <summary>
    /// Get the specified role for the specified 'integrationId' and 'environment'.
    /// </summary>
    /// <param name="integrationId"></param>
    /// <param name="environment"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<RoleModel?> GetRoleAsync(int integrationId, string environment, string name)
    {
        var response = await this.Client.GetAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/roles/{name}"));
        return await response.HandleResponseAsync<RoleModel?>();
    }

    /// <summary>
    /// Update the specified role with a new name, for the specified 'integrationId' and 'environment'.
    /// </summary>
    /// <param name="integrationId"></param>
    /// <param name="environment"></param>
    /// <param name="name"></param>
    /// <param name="newName"></param>
    /// <returns></returns>
    public async Task<RoleModel?> UpdateRoleAsync(int integrationId, string environment, string name, string newName)
    {
        var role = new RoleModel(newName);
        var response = await this.Client.PostAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/roles/{name}"), CreateBody(role));
        return await response.HandleResponseAsync<RoleModel>();
    }

    /// <summary>
    /// Delete the specified role in the specified 'integrationId' and 'environment'.
    /// </summary>
    /// <param name="integrationId"></param>
    /// <param name="environment"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task DeleteRoleAsync(int integrationId, string environment, string name)
    {
        var response = await this.Client.DeleteAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/roles/{name}"));
        await response.HandleResponseAsync();
    }

    public async Task<IEnumerable<RoleModel>> GetCompositeRolesAsync(int integrationId, string environment, string name)
    {
        var response = await this.Client.GetAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/roles/{name}/composite-roles"));
        var result = await response.HandleResponseAsync<DataResponseModel<RoleModel>>();
        return result.Data;
    }

    public async Task<RoleModel> AddCompositeRoleAsync(int integrationId, string environment, string name, IEnumerable<string> roles)
    {
        var role = roles.Select(r => new RoleModel(r));
        var response = await this.Client.PostAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/roles/{name}/composite-roles"), CreateBody(role));
        return await response.HandleResponseAsync<RoleModel>();
    }

    public async Task<RoleModel?> GetCompositeRoleAsync(int integrationId, string environment, string name, string compositeName)
    {
        var response = await this.Client.GetAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/roles/{name}/composite-roles/{compositeName}"));
        return await response.HandleResponseAsync<RoleModel?>();
    }

    public async Task DeleteCompositeRoleAsync(int integrationId, string environment, string name, string compositeName)
    {
        var response = await this.Client.DeleteAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/roles/{name}/composite-roles/{compositeName}"));
        await response.HandleResponseAsync();
    }
    #endregion

    #region Role Mapping

    public async Task<UserRoleResponseModel> GetRolesForUserAsync(int integrationId, string environment, string username)
    {
        var response = await this.Client.GetAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/user-role-mappings?username={HttpUtility.UrlEncode(username)}"));
        return await response.HandleResponseAsync<UserRoleResponseModel?>() ?? new UserRoleResponseModel();
    }

    public async Task<UserRoleResponseModel> GetUsersForRoleAsync(int integrationId, string environment, string role)
    {
        var response = await this.Client.GetAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/user-role-mappings?roleName={HttpUtility.UrlEncode(role)}"));
        return await response.HandleResponseAsync<UserRoleResponseModel>();
    }

    public async Task<UserRoleResponseModel?> AddUserRoleAsync(int integrationId, string environment, string username, string role)
    {
        var map = new UserRoleModel(username, role, UserRoleOperation.Add);
        var response = await this.Client.PostAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/user-role-mappings"), CreateBody(map));
        return await response.HandleResponseAsync<UserRoleResponseModel?>();
    }

    public async Task DeleteUserRoleAsync(int integrationId, string environment, string username, string role)
    {
        var map = new UserRoleModel(username, role, UserRoleOperation.Delete);
        var response = await this.Client.PostAsync(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/integrations/{integrationId}/{environment}/user-role-mappings"), CreateBody(map));
        await response.HandleResponseAsync();
    }
    #endregion

    #region Users
    public async Task<IEnumerable<UserModel>> GetUsersAsync(string environment, UserFilter filter)
    {
        if (String.IsNullOrWhiteSpace(filter.FirstName) &&
            String.IsNullOrWhiteSpace(filter.LastName) &&
            String.IsNullOrWhiteSpace(filter.Email) &&
            String.IsNullOrWhiteSpace(filter.Guid)) throw new ArgumentException("At least one filter value must be provided.");

        return filter.Provider switch
        {
            IdentityProvider.Idir => await GetIdirUsersAsync(environment, filter),
            IdentityProvider.IdirAzure => await GetAzureIdirUsersAsync(environment, filter),
            IdentityProvider.GithubPublic => await GetGithubPublicUsersAsync(environment, filter),
            IdentityProvider.GithubBcGov => await GetGithubBcGovUsersAsync(environment, filter),
            IdentityProvider.BceidBasic => await GetBasicBceidUsersAsync(environment, filter),
            IdentityProvider.BceidBusiness => await GetBusinessBceIdUsersAsync(environment, filter),
            IdentityProvider.BceidBasicBusiness => await GetBasicBusinessBceidUsersAsync(environment, filter),
            _ => throw new ArgumentException("Invalid provider"),
        };
    }

    private async Task<IEnumerable<UserModel>> GetIdirUsersAsync(string environment, UserFilter filter)
    {
        var uri = new UriBuilder(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/{environment}/idir/users"));
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (!String.IsNullOrWhiteSpace(filter.FirstName)) query.Add("firstName", HttpUtility.UrlEncode(filter.FirstName));
        if (!String.IsNullOrWhiteSpace(filter.LastName)) query.Add("lastName", HttpUtility.UrlEncode(filter.LastName));
        if (!String.IsNullOrWhiteSpace(filter.Email)) query.Add("email", HttpUtility.UrlEncode(filter.Email));
        if (!String.IsNullOrWhiteSpace(filter.Guid)) query.Add("guid", HttpUtility.UrlEncode(filter.Guid));
        uri.Query = query.ToString();
        var response = await this.Client.GetAsync(uri.Uri);
        var result = await response.HandleResponseAsync<DataResponseModel<UserModel>>();
        return result.Data;
    }

    private async Task<IEnumerable<UserModel>> GetAzureIdirUsersAsync(string environment, UserFilter filter)
    {
        var uri = new UriBuilder(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/{environment}/azure-idir/users"));
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (!String.IsNullOrWhiteSpace(filter.FirstName)) query.Add("firstName", HttpUtility.UrlEncode(filter.FirstName));
        if (!String.IsNullOrWhiteSpace(filter.LastName)) query.Add("lastName", HttpUtility.UrlEncode(filter.LastName));
        if (!String.IsNullOrWhiteSpace(filter.Email)) query.Add("email", HttpUtility.UrlEncode(filter.Email));
        if (!String.IsNullOrWhiteSpace(filter.Guid)) query.Add("guid", HttpUtility.UrlEncode(filter.Guid));
        uri.Query = query.ToString();
        var response = await this.Client.GetAsync(uri.Uri);
        var result = await response.HandleResponseAsync<DataResponseModel<UserModel>>();
        return result.Data;
    }

    private async Task<IEnumerable<UserModel>> GetGithubBcGovUsersAsync(string environment, UserFilter filter)
    {
        var uri = new UriBuilder(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/{environment}/github-bcgov/users"));
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (!String.IsNullOrWhiteSpace(filter.FirstName)) query.Add("firstName", HttpUtility.UrlEncode(filter.FirstName));
        if (!String.IsNullOrWhiteSpace(filter.LastName)) query.Add("lastName", HttpUtility.UrlEncode(filter.LastName));
        if (!String.IsNullOrWhiteSpace(filter.Email)) query.Add("email", HttpUtility.UrlEncode(filter.Email));
        if (!String.IsNullOrWhiteSpace(filter.Guid)) query.Add("guid", HttpUtility.UrlEncode(filter.Guid));
        uri.Query = query.ToString();
        var response = await this.Client.GetAsync(uri.Uri);
        var result = await response.HandleResponseAsync<DataResponseModel<UserModel>>();
        return result.Data;
    }

    private async Task<IEnumerable<UserModel>> GetGithubPublicUsersAsync(string environment, UserFilter filter)
    {
        var uri = new UriBuilder(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/{environment}/github-public/users"));
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (!String.IsNullOrWhiteSpace(filter.FirstName)) query.Add("firstName", HttpUtility.UrlEncode(filter.FirstName));
        if (!String.IsNullOrWhiteSpace(filter.LastName)) query.Add("lastName", HttpUtility.UrlEncode(filter.LastName));
        if (!String.IsNullOrWhiteSpace(filter.Email)) query.Add("email", HttpUtility.UrlEncode(filter.Email));
        if (!String.IsNullOrWhiteSpace(filter.Guid)) query.Add("guid", HttpUtility.UrlEncode(filter.Guid));
        uri.Query = query.ToString();
        var response = await this.Client.GetAsync(uri.Uri);
        var result = await response.HandleResponseAsync<DataResponseModel<UserModel>>();
        return result.Data;
    }

    private async Task<IEnumerable<UserModel>> GetBasicBceidUsersAsync(string environment, UserFilter filter)
    {
        var uri = new UriBuilder(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/{environment}/basic-bceid/users"));
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (!String.IsNullOrWhiteSpace(filter.FirstName)) query.Add("firstName", HttpUtility.UrlEncode(filter.FirstName));
        if (!String.IsNullOrWhiteSpace(filter.LastName)) query.Add("lastName", HttpUtility.UrlEncode(filter.LastName));
        if (!String.IsNullOrWhiteSpace(filter.Email)) query.Add("email", HttpUtility.UrlEncode(filter.Email));
        if (!String.IsNullOrWhiteSpace(filter.Guid)) query.Add("guid", HttpUtility.UrlEncode(filter.Guid));
        uri.Query = query.ToString();
        var response = await this.Client.GetAsync(uri.Uri);
        var result = await response.HandleResponseAsync<DataResponseModel<UserModel>>();
        return result.Data;
    }

    private async Task<IEnumerable<UserModel>> GetBusinessBceIdUsersAsync(string environment, UserFilter filter)
    {
        var uri = new UriBuilder(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/{environment}/business-bceid/users"));
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (!String.IsNullOrWhiteSpace(filter.FirstName)) query.Add("firstName", HttpUtility.UrlEncode(filter.FirstName));
        if (!String.IsNullOrWhiteSpace(filter.LastName)) query.Add("lastName", HttpUtility.UrlEncode(filter.LastName));
        if (!String.IsNullOrWhiteSpace(filter.Email)) query.Add("email", HttpUtility.UrlEncode(filter.Email));
        if (!String.IsNullOrWhiteSpace(filter.Guid)) query.Add("guid", HttpUtility.UrlEncode(filter.Guid));
        uri.Query = query.ToString();
        var response = await this.Client.GetAsync(uri.Uri);
        var result = await response.HandleResponseAsync<DataResponseModel<UserModel>>();
        return result.Data;
    }

    private async Task<IEnumerable<UserModel>> GetBasicBusinessBceidUsersAsync(string environment, UserFilter filter)
    {
        var uri = new UriBuilder(new Uri(this.Client.ClientOptions.ApiUrl, $"{BASE_URL}/{environment}/basic-business-bceid/users"));
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (!String.IsNullOrWhiteSpace(filter.FirstName)) query.Add("firstName", HttpUtility.UrlEncode(filter.FirstName));
        if (!String.IsNullOrWhiteSpace(filter.LastName)) query.Add("lastName", HttpUtility.UrlEncode(filter.LastName));
        if (!String.IsNullOrWhiteSpace(filter.Email)) query.Add("email", HttpUtility.UrlEncode(filter.Email));
        if (!String.IsNullOrWhiteSpace(filter.Guid)) query.Add("guid", HttpUtility.UrlEncode(filter.Guid));
        uri.Query = query.ToString();
        var response = await this.Client.GetAsync(uri.Uri);
        var result = await response.HandleResponseAsync<DataResponseModel<UserModel>>();
        return result.Data;
    }
    #endregion
    #endregion
}
