using Oracle.ManagedDataAccess.Client;
using TNO.Services.ContentMigration.Config;

namespace TNO.Services.ContentMigration.Sources.Oracle.Services;

/// <summary>
/// Helper for generating Oracle connection string
/// </summary>
public static class OracleConnectionStringHelper
{
    /// <summary>
    /// Generates an Oracle connection string
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public static string GetConnectionString(OracleConnectionSettings settings)
    {
        return GetConnectionString(settings.UserName, settings.Password, settings.HostName, settings.Port, settings.Sid);
    }

    /// <summary>
    /// Generates an Oracle connection string
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="hostName"></param>
    /// <param name="portNumber"></param>
    /// <param name="defaultDb"></param>
    /// <returns></returns>
    public static string GetConnectionString(string userName, string password, string hostName, int? portNumber, string defaultDb)
    {
        var connectionString = portNumber != null
            ?$"Data Source={hostName}:{portNumber}/{defaultDb}"
            : $"Data Source={hostName}/{defaultDb}";
        var oracleBuilder = new OracleConnectionStringBuilder(connectionString)
        {
            UserID = userName,
            Password = password
        };

        return oracleBuilder.ConnectionString;
    }
}
