using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("tone_pool")]
public class TonePool : BaseType<int>
{
    #region Properties
    [Column("owner_id")]
    public int OwnerId { get; set; }

    public virtual User? Owner { get; set; }


    [Column("is_public")]
    public bool IsPublic { get; set; } = false;

    public virtual List<Content> Contents { get; } = new List<Content>();

    public virtual List<ContentTonePool> ContentsManyToMany { get; } = new List<ContentTonePool>();
    #endregion

    #region Constructors
    protected TonePool() { }

    public TonePool(string name, User owner) : base(name)
    {
        this.OwnerId = owner?.Id ?? throw new ArgumentNullException(nameof(owner));
        this.Owner = owner;
    }

    public TonePool(string name, int ownerId) : base(name)
    {
        this.OwnerId = ownerId;
    }
    #endregion
}
