namespace TNO.Elastic.Migration;

/// <summary>
/// Migration_100 class, provides a way to migration elastic to version 1.0.0.
/// </summary>
[Migration("1.0.0")]
public class Migration_100 : Migration
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Migration_100 object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    public Migration_100(MigrationBuilder builder) : base(builder)
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
