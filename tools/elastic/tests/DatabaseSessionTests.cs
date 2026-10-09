using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using TNO.DAL;
using TNO.Elastic.Migration;
using Xunit;

namespace TNO.Elastic.Tests;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("MMI_MIGRATION_TEST_POSTGRES") == null)
            Skip = "Set MMI_MIGRATION_TEST_POSTGRES to a disposable local PostgreSQL.";
    }
}

public sealed class DatabaseSessionTests
{
    private static string ConnectionString => Environment.GetEnvironmentVariable("MMI_MIGRATION_TEST_POSTGRES")!;

    // A key no other test or migration uses, so tests can run side by side.
    private readonly long _key = Random.Shared.NextInt64(1, Int64.MaxValue);

    [Fact]
    public void KeepAliveIsAddedWithoutOverridingTheConnectionString()
    {
        var added = new NpgsqlConnectionStringBuilder(DatabaseSession.WithKeepAlive("Host=db;Database=tno;Username=user;Password=secret"));
        Assert.Equal(DatabaseSession.KeepAliveSeconds, added.KeepAlive);
        Assert.Equal(DatabaseSession.KeepAliveSeconds, added.TcpKeepAliveTime);
        Assert.Equal(DatabaseSession.TcpKeepAliveIntervalSeconds, added.TcpKeepAliveInterval);
        Assert.Equal("db", added.Host);
        Assert.Equal("secret", added.Password);

        var kept = new NpgsqlConnectionStringBuilder(DatabaseSession.WithKeepAlive("Host=db;Keepalive=5;Tcp Keepalive Time=60;Tcp Keepalive Interval=3"));
        Assert.Equal(5, kept.KeepAlive);
        Assert.Equal(60, kept.TcpKeepAliveTime);
        Assert.Equal(3, kept.TcpKeepAliveInterval);
    }

    [PostgresFact]
    public async Task LostSessionDoesNotHideTheMigrationError()
    {
        await using var context = new TNOContext(new DbContextOptionsBuilder<TNOContext>().UseNpgsql(ConnectionString).Options);
        await using var admin = new NpgsqlConnection(ConnectionString);
        await admin.OpenAsync();

        var error = await Assert.ThrowsAsync<TimeoutException>(() => DatabaseSession.RunLockedAsync(context, _key, NullLogger.Instance, async () =>
        {
            // The database session is cut while the migration works, as an idle network timeout would.
            await using var pid = context.Database.GetDbConnection().CreateCommand();
            pid.CommandText = "SELECT pg_backend_pid()";
            await using var terminate = new NpgsqlCommand($"SELECT pg_terminate_backend({await pid.ExecuteScalarAsync()})", admin);
            await terminate.ExecuteScalarAsync();
            throw new TimeoutException("the migration's own error");
        }));
        Assert.Equal("the migration's own error", error.Message);

        // The lock ended with the session, so the next run takes it.
        var ran = false;
        await DatabaseSession.RunLockedAsync(context, _key, NullLogger.Instance, () => { ran = true; return Task.CompletedTask; });
        Assert.True(ran);
    }

    [PostgresFact]
    public async Task SecondRunIsRefusedWhileTheLockIsHeld()
    {
        await using var context = new TNOContext(new DbContextOptionsBuilder<TNOContext>().UseNpgsql(ConnectionString).Options);
        await using var other = new NpgsqlConnection(ConnectionString);
        await other.OpenAsync();
        await using var hold = new NpgsqlCommand($"SELECT pg_advisory_lock({_key})", other);
        await hold.ExecuteScalarAsync();

        var ran = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            DatabaseSession.RunLockedAsync(context, _key, NullLogger.Instance, () => { ran = true; return Task.CompletedTask; }));
        Assert.False(ran);
    }
}
