using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.DAL;

namespace TNO.Test.DAL;

/// <summary>
/// TnoTestContext class, provides a test wrapper for the TnoContext class to handle InMemory database limitations.
/// </summary>
public class TnoTestContext : TNOContext
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a TnoTestContext object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public TnoTestContext(DbContextOptions<TNOContext> options, IHttpContextAccessor? httpContextAccessor = null, IOptions<JsonSerializerOptions>? serializerOptions = null, ILogger<TNOContext>? logger = null)
        : base(options, httpContextAccessor, serializerOptions, logger)
    { }
    #endregion

    #region Methods
    /// <summary>
    /// Create the database model.
    /// Handle the JsonDocument conversion.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // modelBuilder.Entity<Entities.WorkOrder>().Property(p => p.SomeWellDefinedJsonProperty)
        //     .HasConversion(v => JsonConvert.SerializeObject(v),
        //         v => JsonConvert.DeserializeObject<WellDefinedJsonProperty>(v));

        // TODO: Find a way to automate this so that we don't have to manually add them each time.
        modelBuilder.Entity<Entities.Content>().Property(p => p.Versions)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
                v => JsonSerializer.Deserialize<Dictionary<int, Entities.Models.ContentVersion>>(v, JsonSerializerOptions.Default) ?? new());

        modelBuilder.Entity<Entities.Connection>().Property(p => p.Configuration)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.Ingest>().Property(p => p.Configuration)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.Source>().Property(p => p.Configuration)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.WorkOrder>().Property(p => p.Configuration)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.User>().Property(p => p.Preferences)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.AVOverviewInstance>().Property(p => p.Response)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.Folder>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.Filter>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.Filter>().Property(p => p.Query)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.Report>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.ReportSection>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.ReportTemplate>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.ChartTemplate>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.ReportSectionChartTemplate>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.ReportInstance>().Property(p => p.Response)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.Notification>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.Notification>().Property(p => p.Query)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.NotificationInstance>().Property(p => p.Response)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.NotificationTemplate>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.EventSchedule>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.ContentReference>().Property(p => p.Metadata)
            .HasConversion(
                v => JsonDocumentToString(v!), //KGM: Ignoring this for TestContext
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.MediaType>().Property(p => p.Settings)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.UserReportInstance>().Property(p => p.LinkResponse)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.UserReportInstance>().Property(p => p.TextResponse)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));

        modelBuilder.Entity<Entities.UserAVOverviewInstance>().Property(p => p.Response)
            .HasConversion(
                v => JsonDocumentToString(v),
                v => JsonDocument.Parse(v, new JsonDocumentOptions()));
    }

    /// <summary>
    /// Deserializes and convert JsonDocument to string.
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    private static string JsonDocumentToString(JsonDocument document)
    {
        using var stream = new MemoryStream();
        var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
        document.WriteTo(writer);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
    #endregion
}
