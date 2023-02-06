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
