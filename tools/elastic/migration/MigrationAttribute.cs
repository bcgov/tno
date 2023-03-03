namespace TNO.Elastic.Migration;

/// <summary>
/// MigrationAttribute class, provides a way to identify the migration version name.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MigrationAttribute : Attribute
{
    #region Properties
    /// <summary>
    /// get/set - The name of the migration (the version).
    /// </summary>
    public string Id { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MigrationAttribute object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    public MigrationAttribute(string id)
    {
        this.Id = id;
    }
    #endregion
}
