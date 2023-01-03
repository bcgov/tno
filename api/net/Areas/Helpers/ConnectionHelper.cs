using System.Security.Claims;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Models.Extensions;

namespace TNO.API.Helpers;

/// <summary>
/// ConnectionHelper class, provides a helper class that creates SSH clients for a specified data location.
/// </summary>
public class ConnectionHelper : IConnectionHelper
{
    #region Variables
    private readonly IDataLocationService _dataLocationService;
    private readonly JsonSerializerOptions _serializationOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ConnectionHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataLocationService"></param>
    /// <param name="serializationOptions"></param>
    public ConnectionHelper(IDataLocationService dataLocationService, IOptions<JsonSerializerOptions> serializationOptions)
    {
        _dataLocationService = dataLocationService;
        _serializationOptions = serializationOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the connection for the specified data location.
    /// </summary>
    /// <param name="locationId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Connection? GetConnection(int locationId)
    {
        return _dataLocationService.FindById(locationId)?.Connection;
    }

    /// <summary>
    /// Get the connection for the specified data location.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Connection? GetConnection(string location)
    {
        return _dataLocationService.FindByName(HttpUtility.UrlDecode(location))?.Connection;
    }

    /// <summary>
    /// Fetch the DataLocation from the data source and convert the connection configuration to a dictionary.
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Dictionary<string, object> GetConfiguration(Connection connection)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object>>(connection.Configuration, _serializationOptions) ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Create a new SftpClient object for the specified configuration.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    public SftpClient CreateSftpClient(Dictionary<string, object> configuration)
    {
        // TODO: Handle different authentication methods.
        var hostname = configuration.GetConfigurationValue<string>("hostname") ?? throw new ConfigurationException("Connection configuration 'hostname' is required.");
        var username = configuration.GetConfigurationValue<string>("username") ?? throw new ConfigurationException("Connection configuration 'username' is required.");
        var password = configuration.GetConfigurationValue<string>("password") ?? throw new ConfigurationException("Connection configuration 'password' is required.");
        var port = int.Parse(configuration.GetConfigurationValue<string>("port") ?? "22");

        var authMethod = new PasswordAuthenticationMethod(username, password);
        var connectionInfo = new Renci.SshNet.ConnectionInfo(hostname, port, username, authMethod);
        return new SftpClient(connectionInfo);
    }

    /// <summary>
    /// Copy the file from the remote location to the local destination path.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="srcFilePath"></param>
    /// <param name="destFolderPath"></param>
    /// <param name="user"></param>
    /// <returns>Path to local file</returns>
    public string CopyFile(SftpClient client, string srcFilePath, string destFolderPath, ClaimsPrincipal user)
    {
        var tmpPath = Path.Combine(destFolderPath, "_tmp", user.GetUsername() ?? "");
        if (!Directory.Exists(tmpPath))
            Directory.CreateDirectory(tmpPath);

        var tmpFilePath = Path.Combine(tmpPath, $"{Guid.NewGuid()}{Path.GetExtension(srcFilePath)}");
        using var copyStream = System.IO.File.OpenWrite(tmpFilePath);
        client.DownloadFile(srcFilePath, copyStream);
        copyStream.Close();

        return tmpFilePath;
    }
    #endregion
}
