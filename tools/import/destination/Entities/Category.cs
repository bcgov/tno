using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("category")]
public class Category : BaseType<int>
{
    #region Properties

    public List<ContentCategory> ContentCategories { get; } = new List<ContentCategory>();
    #endregion

    #region Constructors
    protected Category() { }

    public Category(string name) : base(name)
    {
    }
    #endregion
}