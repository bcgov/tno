using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;
/// <summary>
/// Minister class, provides a database entity model to manage a list of ministers.
/// </summary>
[Cache("minister", "lookups")]
[Table("minister")]
public class Minister : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Comma separated list of alias values when searching for the minister.
    /// </summary>
    [Column("aliases")]
    public string Aliases { get; set; } = "";

    /// <summary>
    /// get/set - The minister's position title.
    /// </summary>
    [Column("position")]
    public string Position { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to organization this minster belongs to.
    /// </summary>
    [Column("organization_id")]
    public int? OrganizationId { get; set; }

    /// <summary>
    /// get/set - The organization this minister belong to.
    /// </summary>
    public virtual Organization? Organization { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Minister object.
    /// </summary>
    protected Minister() { }

    /// <summary>
    /// Creates a new instance of a Minister object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="organization"></param>
    public Minister(string name, Organization? organization = null) : base(name)
    {
        this.Organization = organization;
        this.OrganizationId = organization?.Id;
    }

    /// <summary>
    /// Creates a new instance of a Minister object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="organizationId"></param>
    public Minister(int id, string name, int? organizationId) : base(id, name)
    {
        this.OrganizationId = organizationId;
    }
    #endregion
}
