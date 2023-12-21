using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.DAL.Services;

namespace TNO.Elastic.Migration;

/// <summary>
/// Migration_1010 class, provides a way to migration elastic to version 1.0.10.
/// </summary>
[Migration("1.0.10")]
public class Migration_1010 : TNOMigration
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a Migration_1010 object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="contentService"></param>
    /// <param name="serializerOptions"></param>
    public Migration_1010(MigrationBuilder builder, IContentService contentService, IOptions<JsonSerializerOptions> serializerOptions) : base(builder, contentService, serializerOptions)
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
