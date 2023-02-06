using Renci.SshNet;
using TNO.Core.Exceptions;
using TNO.Models.Extensions;

namespace TNO.Services;

/// <summary>
/// SftpHelper class, provides helper methods for SFTP processes.
/// </summary>
public class SftpHelper
{
    #region Constructors
    #endregion

    #region Methods
    /// <summary>
    /// Create a new SftpClient object for the specified configuration.
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    public static SftpClient CreateSftpClient(Dictionary<string, object> configuration)
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
    /// <param name="srcPath"></param>
    /// <param name="destPath"></param>
    /// <returns>Path to local file</returns>
    public static void CopyFile(SftpClient client, string srcPath, string destPath)
    {
        using var copyStream = System.IO.File.OpenWrite(destPath);
        client.DownloadFile(srcPath, copyStream);
        copyStream.Close();
    }
    #endregion
}
