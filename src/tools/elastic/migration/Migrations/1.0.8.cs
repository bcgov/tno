using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.DAL.Services;

namespace TNO.Elastic.Migration;

/// <summary>
/// Migration_108 class, provides a way to migration elastic to version 1.0.8.
/// </summary>
[Migration("1.0.8")]
public class Migration_108 : TNOMigration
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a Migration_108 object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="contentService"></param>
    /// <param name="serializerOptions"></param>
    public Migration_108(MigrationBuilder builder, IContentService contentService, IOptions<JsonSerializerOptions> serializerOptions) : base(builder, contentService, serializerOptions)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    protected override Task UpAsync(MigrationBuilder builder)
    {
        return Task.CompletedTask;
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
