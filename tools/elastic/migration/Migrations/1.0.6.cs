using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.DAL.Services;
using TNO.Models.Filters;

namespace TNO.Elastic.Migration;

/// <summary>
/// Migration_106 class, provides a way to migration elastic to version 1.0.6.
/// </summary>
[Migration("1.0.6")]
public class Migration_106 : TNOMigration
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a Migration_105 object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="contentService"></param>
    /// <param name="serializerOptions"></param>
    public Migration_106(MigrationBuilder builder, IContentService contentService, IOptions<JsonSerializerOptions> serializerOptions) : base(builder, contentService, serializerOptions)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    protected override async Task UpAsync(MigrationBuilder builder)
    {
        await ReindexAsync(builder, new ContentFilter());
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    protected override Task DownAsync(MigrationBuilder builder)
    {
        return Task.CompletedTask;
    }
    #endregion
}
