using System.Security.Claims;
using Renci.SshNet;
using TNO.Entities;

namespace TNO.API.Helpers;

/// <summary>
///
/// </summary>
public interface IConnectionHelper
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="locationId"></param>
    /// <returns></returns>
    Connection? GetConnection(int locationId);

    /// <summary>
    ///
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    Connection? GetConnection(string location);

    /// <summary>
    ///
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    Dictionary<string, object> GetConfiguration(Connection connection);

    /// <summary>
    ///
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    SftpClient CreateSftpClient(Dictionary<string, object> configuration);

    /// <summary>
    ///
    /// </summary>
    /// <param name="client"></param>
    /// <param name="srcFilePath"></param>
    /// <param name="destFolderPath"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    string CopyFile(SftpClient client, string srcFilePath, string destFolderPath, ClaimsPrincipal user);
}
