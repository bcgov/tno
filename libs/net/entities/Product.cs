using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Product class, provides an entity model to manage different products, group related content, and display content to subscribers.
/// </summary>
[Cache("products", "lookups")]
[Table("product")]
public class Product : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get - List of ingest linked to this product.
    /// </summary>
    public virtual List<Ingest> Ingests { get; } = new List<Ingest>();

    /// <summary>
    /// get - List of content linked to this product.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Product object.
    /// </summary>
    protected Product() { }

    /// <summary>
    /// Creates a new instance of a Product object, initializes with specified parameter.
    /// </summary>
    /// <param name="name"></param>
    public Product(string name) : base(name)
    {
    }
    #endregion
}
