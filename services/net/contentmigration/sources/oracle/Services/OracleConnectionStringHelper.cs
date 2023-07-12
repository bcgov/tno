using Oracle.ManagedDataAccess.Client;

namespace TNO.Services.ContentMigration.Sources.Oracle.Services;

/// <summary>
/// Helper for generating Oracle connection string
/// </summary>
public static class OracleConnectionStringHelper
{
    /// <summary>
    /// Generates an Oracle connection string
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    public static string GetConnectionString(string userName, string password, string dataSource)
    {

        var oracleBuilder = new OracleConnectionStringBuilder(dataSource)
        {
            UserID = userName,
            Password = password
        };

        return oracleBuilder.ConnectionString;
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
    public static string GetConnectionString(string userName, string password, string hostName, int portNumber, string defaultDb)
    {

        var oracleBuilder = new OracleConnectionStringBuilder($"Data Source={hostName}:{portNumber}/{defaultDb}")
        {
            UserID = userName,
            Password = password
        };

        return oracleBuilder.ConnectionString;
    }
}
