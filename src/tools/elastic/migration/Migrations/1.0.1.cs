namespace TNO.Elastic.Migration;

/// <summary>
/// Migration_101 class, provides a way to migration elastic to version 1.0.1.
/// </summary>
[Migration("1.0.1")]
public class Migration_101 : Migration
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Migration_101 object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    public Migration_101(MigrationBuilder builder) : base(builder)
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
