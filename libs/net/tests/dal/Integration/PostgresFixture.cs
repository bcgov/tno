using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using TNO.DAL;
using TNO.Entities;

namespace TNO.Test.DAL.Integration;

/// <summary>
/// PostgresFixture class, creates contexts on the integration database and removes the content the
/// tests add (with everything that belongs to it).
/// </summary>
public sealed class PostgresFixture : IDisposable
{
    #region Variables
    private readonly NpgsqlDataSource? _dataSource;
    private readonly List<long> _contentIds = new();
    #endregion

    #region Properties
    /// <summary>
    /// get - An empty service provider for the services under test.
    /// </summary>
    public IServiceProvider Services { get; } = new ServiceCollection().BuildServiceProvider();

    /// <summary>
    /// get - The principal the services act as.
    /// </summary>
    public ClaimsPrincipal Principal { get; } = new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "integration-test") }, "test"));
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a PostgresFixture object.
    /// </summary>
    public PostgresFixture()
    {
        var connectionString = Environment.GetEnvironmentVariable(PostgresFactAttribute.Variable);
        if (String.IsNullOrWhiteSpace(connectionString)) return;
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.EnableDynamicJson();
        _dataSource = builder.Build();
    }
    #endregion

    #region Methods
    /// <summary>
    /// A new context on the integration database.
    /// </summary>
    /// <returns></returns>
    public TNOContext CreateContext()
    {
        if (_dataSource == null) throw new InvalidOperationException($"{PostgresFactAttribute.Variable} is not set.");
        var options = new DbContextOptionsBuilder<TNOContext>().UseNpgsql(_dataSource).Options;
        return new TNOContext(options);
    }

    /// <summary>
    /// Add a content item (removed when the tests finish).
    /// </summary>
    /// <param name="context"></param>
    /// <param name="status"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public Content AddContent(TNOContext context, ContentStatus status = ContentStatus.Draft, System.Action<Content>? configure = null)
    {
        var source = context.Sources.AsNoTracking().OrderBy(s => s.Id).First();
        var mediaTypeId = context.MediaTypes.AsNoTracking().OrderBy(m => m.Id).Select(m => m.Id).First();
        var licenseId = context.Licenses.AsNoTracking().OrderBy(l => l.Id).Select(l => l.Id).First();
        var content = new Content($"it-{Guid.NewGuid():N}", "Integration test headline", source.Code, source.Id, ContentType.PrintContent, licenseId, mediaTypeId)
        {
            Status = status,
            Body = "<p>The minister said the hospital will open in the spring.</p>",
            PublishedOn = DateTime.UtcNow,
        };
        configure?.Invoke(content);
        context.Contents.Add(content);
        context.SaveChanges();
        Track(content.Id);
        return content;
    }

    /// <summary>
    /// Remove this content item when the tests finish.
    /// </summary>
    /// <param name="contentId"></param>
    public void Track(long contentId)
    {
        lock (_contentIds) _contentIds.Add(contentId);
    }

    /// <summary>
    /// Remove the content the tests added.
    /// </summary>
    public void Dispose()
    {
        if (_dataSource == null) return;
        if (_contentIds.Count > 0)
        {
            using var context = CreateContext();
            var ids = _contentIds.ToArray();
            context.Contents.Where(c => ids.Contains(c.Id)).ExecuteDelete();
        }
        _dataSource.Dispose();
    }
    #endregion
}

/// <summary>
/// Integration tests share one database; they run one class at a time.
/// </summary>
[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    /// <summary>The collection name.</summary>
    public const string Name = "Postgres";
}

/// <summary>
/// Null loggers for the services under test.
/// </summary>
internal static class TestLogger
{
    public static Microsoft.Extensions.Logging.ILogger<T> For<T>() => NullLogger<T>.Instance;
}
