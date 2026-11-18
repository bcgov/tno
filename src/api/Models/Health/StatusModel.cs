using System.Reflection;

namespace TNO.API.Models.Health;

/// <summary>
/// EnvModel class, provides a model to represent a user.
/// </summary>
public class StatusModel
{
    #region Properties
    /// <summary>
    /// get/set - The status.
    /// </summary>
    public string Status { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a StatusModel object.
    /// </summary>
    public StatusModel() { }

    /// <summary>
    /// Creates a new instance of a StatusModel object, initializes it with specified arguments.
    /// </summary>
    /// <param name="status"></param>
    public StatusModel(string status)
    {
        this.Status = status;
    }
    #endregion
}
