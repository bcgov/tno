namespace TNO.API.Areas.Services.Models.Contributor;

/// <summary>
/// ContributorModel class, a minimal contributor for service-to-service calls.
/// </summary>
public class ContributorModel
{
    #region Properties
    /// <summary>get/set - Primary key.</summary>
    public int Id { get; set; }

    /// <summary>get/set - The contributor name.</summary>
    public string Name { get; set; } = "";

    /// <summary>get/set - Comma-separated aliases.</summary>
    public string? Aliases { get; set; }
    #endregion

    #region Constructors
    /// <summary>Creates a new instance of a ContributorModel object.</summary>
    public ContributorModel() { }

    /// <summary>Creates a new instance of a ContributorModel object from the entity.</summary>
    /// <param name="entity"></param>
    public ContributorModel(Entities.Contributor entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Aliases = entity.Aliases;
    }
    #endregion
}
