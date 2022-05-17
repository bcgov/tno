using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Claim;

/// <summary>
/// ClaimModel class, provides a model that represents an claim.
/// </summary>
public class ClaimModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to claim.
    /// </summary>
    public int Id { get; set; } = default!;

    /// <summary>
    /// get/set - Unique key to identify the claim.
    /// </summary>
    public Guid Key { get; set; } = Guid.Empty;

    /// <summary>
    /// get/set - Unique name to identify claim.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Description of claim.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether the claim is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ClaimModel.
    /// </summary>
    public ClaimModel() { }

    /// <summary>
    /// Creates a new instance of an ClaimModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ClaimModel(Entities.Claim entity)
    {
        this.Id = entity.Id;
        this.Key = entity.Key;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
    }
    #endregion
}
