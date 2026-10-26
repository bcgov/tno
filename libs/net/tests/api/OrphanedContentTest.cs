using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Nest;
using Npgsql;
using TNO.API.Areas.Admin.Controllers;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Helpers;
using TNO.Core.Exceptions;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Entities;
using TNO.Keycloak;
using Xunit;

namespace TNO.Test.API;

// A disposable database is created for this class. Never points at application tables.
public partial class OrphanedContentTest : IAsyncLifetime
{
    private NpgsqlDataSource _source = null!;
    private string _admin = "";
    private readonly string _database = "orphan_test_" + Guid.NewGuid().ToString("N");
    // Each test owns a separate database/data source. Keep its EF provider local instead of
    // accumulating a new singleton data source in EF's process-wide provider cache.
    private readonly ServiceProvider _efServices = new ServiceCollection().AddEntityFrameworkNpgsql().BuildServiceProvider();
    private readonly IServiceProvider _services = new ServiceCollection().BuildServiceProvider();
    private readonly ClaimsPrincipal _user = new(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "orphan-test") }, "test"));
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    private int _license, _media;

    public async Task InitializeAsync()
    {
        _admin = Environment.GetEnvironmentVariable("TNO_ORPHAN_TEST_POSTGRES") ?? "";
        if (_admin.Length == 0) return;
        await using var connection = new NpgsqlConnection(_admin);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE {_database}", connection);
        await command.ExecuteNonQueryAsync();
        var builder = new NpgsqlDataSourceBuilder(new NpgsqlConnectionStringBuilder(_admin) { Database = _database }.ConnectionString);
        builder.EnableDynamicJson();
        _source = builder.Build();
        await using var context = Context();
        await context.Database.EnsureCreatedAsync();
        // EnsureCreated uses the current model. Return to the pre-PR schema before
        // applying the complete migration, including its concurrent content index.
        await ApplyMigrationAsync(context, new TNO.DAL.Migrations._1513().DownOperations);
        await ApplyMigrationAsync(context, new TNO.DAL.Migrations._1513().UpOperations);
        var license = new TNO.Entities.License("Test license", 0);
        var media = new MediaType("Test media");
        context.AddRange(license, media);
        context.SaveChanges();
        _license = license.Id; _media = media.Id;
    }

    public async Task DisposeAsync()
    {
        if (_admin.Length == 0) return;
        await _efServices.DisposeAsync();
        await _source.DisposeAsync();
        await using var connection = new NpgsqlConnection(_admin);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE {_database} WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task ApplyMigrationAsync(TNOContext context, IReadOnlyList<Microsoft.EntityFrameworkCore.Migrations.Operations.MigrationOperation> operations)
    {
        var commands = context.GetService<IMigrationsSqlGenerator>().Generate(operations, context.Model);
        foreach (var command in commands) await context.Database.ExecuteSqlRawAsync(command.CommandText);
    }

    private TNOContext Context() => new(new DbContextOptionsBuilder<TNOContext>().UseNpgsql(_source).UseInternalServiceProvider(_efServices).Options);
    private Content Content(TNOContext context, bool isPrivate = false, ContentStatus status = ContentStatus.Published)
    {
        var content = new Content(Guid.NewGuid().ToString(), "Test story", "Test", null, ContentType.PrintContent, _license, _media)
        { IsPrivate = isPrivate, Status = status };
        context.Contents.Add(content); context.SaveChanges(); return content;
    }

    private (OrphanedContentService Service, Mock<ITNOElasticClient> Elastic) Service(TNOContext context, object[] documents, bool timedOut = false)
    {
        var body = JsonSerializer.Serialize(new { timed_out = timedOut, _shards = new { failed = 0 }, hits = new { hits = documents } }, _json);
        var connection = new InMemoryConnection(Encoding.UTF8.GetBytes(body));
        var client = new ElasticClient(new ConnectionSettings(new SingleNodeConnectionPool(new Uri("http://localhost:9200")), connection).DisablePing());
        var elastic = new Mock<ITNOElasticClient>();
        elastic.SetupGet(c => c.LowLevel).Returns(client.LowLevel);
        var contentService = new ContentService(context, _user, _services, elastic.Object, NullLogger<ContentService>.Instance);
        return (new OrphanedContentService(context, contentService, elastic.Object, Options.Create(new ElasticOptions()), Options.Create(_json), NullLogger<OrphanedContentService>.Instance), elastic);
    }

    private object Hit(ContentModel model, long revision = 5) => new { _id = model.Id.ToString(), _version = revision, _source = model };

    [Fact]
    public void EndpointsRequireAdministrator()
    {
        var attribute = Assert.Single(typeof(OrphanedContentController).GetCustomAttributes(typeof(ClientRoleAuthorizeAttribute), true));
        Assert.Equal(new[] { ClientRole.Administrator }, ClientRoleAuthorizeAttribute.GetRolesFromPolicy(((ClientRoleAuthorizeAttribute)attribute).Policy!));
    }

    [DatabaseFact]
    public async Task RestorePreservesIdAdvancesRevisionAndQueuesIndexing()
    {
        await using var context = Context();
        var id = 1000000L;
        var model = new ContentModel { Id = id, Uid = "restore-test", Headline = "Restored", OtherSource = "Test", LicenseId = _license, MediaTypeId = _media, Status = ContentStatus.Published, Body = "Full body", ExternalUid = "external" };
        var (service, _) = Service(context, [Hit(model, 8)]);
        await service.RestoreAsync(id, "content", default);
        await using var verification = Context();
        var restored = await verification.Contents.FindAsync(id);
        Assert.NotNull(restored); Assert.Equal("Full body", restored.Body); Assert.Equal("external", restored.ExternalUid); Assert.Equal(9, restored.ProjectionRevision);
        Assert.Equal(id, Assert.Single(context.TakeIndexRequests()).ContentId);
        Assert.True(Content(verification).Id > id);
    }

    [DatabaseFact]
    public async Task RestoreRejectsMissingRequiredLookupWithoutInserting()
    {
        await using var context = Context();
        var model = new ContentModel { Id = 2000000, LicenseId = int.MaxValue, MediaTypeId = _media, Headline = "Missing license" };
        var (service, _) = Service(context, [Hit(model)]);
        var error = await Assert.ThrowsAsync<BadRequestException>(() => service.RestoreAsync(model.Id, "content", default));
        Assert.Contains("license", error.Message);
        Assert.False(await context.Contents.AnyAsync(c => c.Id == model.Id));
    }

    [DatabaseFact]
    public async Task StaleRepairCannotOverwriteOrDeleteExistingContent()
    {
        await using var context = Context();
        var content = Content(context);
        var (service, elastic) = Service(context, [Hit(new ContentModel { Id = content.Id })]);
        await Assert.ThrowsAsync<BadRequestException>(() => service.RestoreAsync(content.Id, "content", default));
        await Assert.ThrowsAsync<BadRequestException>(() => service.DeleteAsync(content.Id, "elasticsearch", "content", default));
        Assert.True(await context.Contents.AnyAsync(c => c.Id == content.Id));
        Assert.DoesNotContain(elastic.Invocations, i => i.Method.Name == "DeleteAsync");
    }

    [DatabaseFact]
    public async Task IndexRepairAndDatabaseDeletionRecordCommittedRequests()
    {
        await using var context = Context();
        var content = Content(context);
        var (service, _) = Service(context, []);
        await service.IndexAsync(content.Id, "content", default);
        Assert.Equal(IndexRequestAction.Publish, Assert.Single(context.TakeIndexRequests()).Action);
        await service.DeleteAsync(content.Id, "postgres", "content", default);
        Assert.Equal(IndexRequestAction.Delete, Assert.Single(context.TakeIndexRequests()).Action);
        Assert.False(await context.Contents.AnyAsync(c => c.Id == content.Id));
    }

    [DatabaseFact]
    public async Task SyncedScanResultsCanDeleteDatabaseContentAndQueueSearchCleanup()
    {
        await using var context = Context();
        foreach (var side in new[] { "postgres", "elasticsearch" })
        {
            var item = Content(context);
            var job = new OrphanScan { Side = side, Status = "Completed", CompletedOn = DateTime.UtcNow };
            context.OrphanScans.Add(job);
            var result = new OrphanScanResult { ScanId = job.Id, ContentId = item.Id, Confirmed = true };
            context.OrphanScanResults.Add(result);
            await context.SaveChangesAsync();
            var (service, _) = Service(context, [Hit(new ContentModel { Id = item.Id })]);
            await Assert.ThrowsAsync<BadRequestException>(() => service.DeleteAsync(item.Id, side, "content", default, job.Id));
            result.Resolution = side == "postgres" ? "Indexing queued" : "Restored";
            await context.SaveChangesAsync();
            await Assert.ThrowsAsync<BadRequestException>(() => service.DeleteAsync(item.Id, side, "content", default, Guid.NewGuid()));
            context.TakeIndexRequests();
            await service.DeleteAsync(item.Id, side, "content", default, job.Id);
            Assert.False(await context.Contents.AnyAsync(x => x.Id == item.Id));
            var request = Assert.Single(context.TakeIndexRequests());
            Assert.Equal(item.Id, request.ContentId);
            Assert.Equal(IndexRequestAction.Delete, request.Action);
        }
    }

    [DatabaseFact]
    public async Task LookupGuardsAndDatabaseConstraintsPreserveContent()
    {
        await using var context = Context();
        var source = new Source("Source", "test", _license);
        var series = new Series("Series");
        context.AddRange(source, series); context.SaveChanges();
        var content = Content(context); content.SourceId = source.Id; content.SeriesId = series.Id; context.SaveChanges();
        Assert.Throws<BadRequestException>(() => new SourceService(context, _user, _services, NullLogger<SourceService>.Instance).Delete(source));
        Assert.Throws<BadRequestException>(() => new SeriesService(context, _user, _services, NullLogger<SeriesService>.Instance).Delete(series));
        Assert.Throws<BadRequestException>(() => new LicenseService(context, _user, _services, NullLogger<LicenseService>.Instance).Delete(context.Licenses.Find(_license)!));
        Assert.Throws<BadRequestException>(() => new MediaTypeService(context, _user, _services, NullLogger<MediaTypeService>.Instance).Delete(context.MediaTypes.Find(_media)!));
        foreach (var (sql, id) in new[]
        {
            ("DELETE FROM public.source WHERE id = {0}", source.Id),
            ("DELETE FROM public.series WHERE id = {0}", series.Id),
            ("DELETE FROM public.license WHERE id = {0}", _license),
            ("DELETE FROM public.media_type WHERE id = {0}", _media),
        })
        {
            var error = await Assert.ThrowsAsync<PostgresException>(() => context.Database.ExecuteSqlRawAsync(sql, id));
            Assert.Equal("23503", error.SqlState);
        }
        Assert.True(await context.Contents.AnyAsync(c => c.Id == content.Id));
    }

    [DatabaseFact]
    public async Task KafkaFailureAlertsSupportAfterDatabaseCommit()
    {
        await using var context = Context();
        var content = Content(context);
        context.RequestIndex(content.Id, IndexRequestAction.Delete);
        context.Contents.Remove(content); context.SaveChanges();
        var kafka = new Mock<TNO.Kafka.IKafkaMessenger>();
        kafka.Setup(k => k.SendMessageAsync("index", It.IsAny<TNO.Kafka.Models.IndexRequestModel>()))
            .ThrowsAsync(new InvalidOperationException("Kafka unavailable"));
        var notifier = new Mock<IIndexFailureNotifier>();
        notifier.Setup(n => n.NotifyAsync(It.IsAny<IEnumerable<SavedIndexRequest>>(), It.IsAny<Exception>())).Returns(Task.CompletedTask);
        var sender = new IndexRequestSender(kafka.Object, Options.Create(new TNO.API.Config.KafkaOptions { IndexingTopic = "index" }), NullLogger<IndexRequestSender>.Instance, notifier.Object);
        await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync(context));
        notifier.Verify(n => n.NotifyAsync(It.Is<IEnumerable<SavedIndexRequest>>(r => r.Any(x => x.ContentId == content.Id && x.Action == IndexRequestAction.Delete)), It.IsAny<Exception>()), Times.Once);
        await using var verification = Context();
        Assert.False(await verification.Contents.AnyAsync(c => c.Id == content.Id));
    }

    [DatabaseFact]
    public async Task ContributorDeletionKeepsContentAndRequestsRefresh()
    {
        await using var context = Context();
        var contributor = new Contributor("Contributor"); context.Add(contributor); context.SaveChanges();
        var content = Content(context); content.ContributorId = contributor.Id; context.SaveChanges();
        new ContributorService(context, _user, _services, NullLogger<ContributorService>.Instance).DeleteAndSave(contributor);
        await using var verification = Context();
        Assert.Null((await verification.Contents.FindAsync(content.Id))!.ContributorId);
        Assert.Equal(content.Id, Assert.Single(context.TakeIndexRequests()).ContentId);
    }
}

public sealed class DatabaseFactAttribute : FactAttribute
{
    public DatabaseFactAttribute()
    {
        if (String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TNO_ORPHAN_TEST_POSTGRES")))
            Skip = "Set TNO_ORPHAN_TEST_POSTGRES to a PostgreSQL admin connection for disposable test databases.";
    }
}
