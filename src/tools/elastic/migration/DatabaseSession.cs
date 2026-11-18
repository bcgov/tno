using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace TNO.Elastic.Migration;

/// <summary>
/// The migration's single database session. A migration opens it once, holds an advisory lock on it
/// for its whole run, and makes every database read through it, including after long waits that use
/// only Elasticsearch (copying, building replicas). A network path that drops idle connections
/// would otherwise cut it silently, and the next read would wait for the full command timeout.
/// </summary>
public static class DatabaseSession
{
    /// <summary>Seconds of inactivity before a keepalive is sent.</summary>
    public const int KeepAliveSeconds = 30;

    /// <summary>Seconds between unanswered TCP keepalives.</summary>
    public const int TcpKeepAliveIntervalSeconds = 10;

    /// <summary>
    /// Add keepalives to a connection string: a keepalive query while the session is idle, and TCP
    /// keepalives while a command waits on the server. Values the connection string already sets are kept.
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static string WithKeepAlive(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (builder.KeepAlive == 0) builder.KeepAlive = KeepAliveSeconds;
        if (builder.TcpKeepAliveTime == 0)
        {
            builder.TcpKeepAliveTime = KeepAliveSeconds;
            if (builder.TcpKeepAliveInterval == 0) builder.TcpKeepAliveInterval = TcpKeepAliveIntervalSeconds;
        }
        return builder.ConnectionString;
    }

    /// <summary>
    /// Run the action holding the advisory lock 'key' on the context's connection, so only one
    /// migration runs against the database. Failing to release the lock never hides the action's
    /// own error: a session that is gone has already released it.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="key"></param>
    /// <param name="logger"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Another migration holds the lock.</exception>
    public static async Task RunLockedAsync(DbContext context, long key, ILogger logger, Func<Task> action)
    {
        await context.Database.OpenConnectionAsync();
        try
        {
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"SELECT pg_try_advisory_lock({key})";
            if (!Equals(await command.ExecuteScalarAsync(), true))
                throw new InvalidOperationException("Another Elasticsearch migration is using this database.");
            try { await action(); }
            finally
            {
                try
                {
                    command.CommandText = $"SELECT pg_advisory_unlock({key})";
                    await command.ExecuteScalarAsync();
                }
                catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
                {
                    logger.LogWarning(ex, "Could not release the migration's database lock; the database releases it when it ends the session.");
                }
            }
        }
        finally { await context.Database.CloseConnectionAsync(); }
    }
}
