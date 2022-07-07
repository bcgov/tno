using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using TNO.DAL;

namespace TNO.Test.DAL;

public static class DbContextHelper
{
    #region Methods
    /// <summary>
    /// Create and configure a new instance of a TNOContext object.
    /// Creates an inmemory database with the specified 'name'.
    /// </summary>
    /// <param name="dbName"></param>
    /// <returns></returns>
    public static TNOContext Build(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<TNOContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .ConfigureWarnings(o => o.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new TNOContext(options);
    }

    /// <summary>
    /// Create and configure a new instance of a TNOContext object.
    /// Creates an inmemory database with the specified 'name'.
    /// Adds a singleton to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="dbName"></param>
    /// <returns></returns>
    public static IServiceCollection AddTNOContext(this IServiceCollection services, string? dbName = null)
    {
        return services.AddSingleton(Build(dbName));
    }

    /// <summary>
    /// Ensure the database has been deleted.
    /// </summary>
    /// <param name="context"></param>
    public static void EnsureDeleted(this DbContext context)
    {
        context.Database.EnsureDeleted();
    }
    #endregion
}
