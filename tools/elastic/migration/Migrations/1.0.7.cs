using TNO.DAL.Models;
using TNO.DAL.Services;

namespace TNO.Elastic.Migration;

/// <summary>
/// Migration_107 class, provides a way to migration elastic to version 1.0.7.
/// </summary>
[Migration("1.0.7")]
public class Migration_107 : TNOMigration
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a Migration_107 object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="contentService"></param>
    public Migration_107(MigrationBuilder builder, IContentService contentService) : base(builder, contentService)
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
