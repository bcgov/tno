namespace TNO.Test.DAL.Integration;

/// <summary>
/// PostgresFactAttribute class, a test that runs against a real PostgreSQL database with the
/// current migrations applied (raw SQL, locking, and constraints cannot be tested in memory).
/// Skipped unless 'TNO_TEST_POSTGRES' holds a connection string, e.g.
/// 'Host=localhost:40000;Database=tno;Username=admin;Password=...'.
/// </summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    /// <summary>
    /// The environment variable that holds the connection string.
    /// </summary>
    public const string Variable = "TNO_TEST_POSTGRES";

    /// <summary>
    /// Creates a new instance of a PostgresFactAttribute object.
    /// </summary>
    public PostgresFactAttribute()
    {
        if (String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(Variable)))
            Skip = $"Set {Variable} to a PostgreSQL connection string to run database integration tests.";
    }
}
