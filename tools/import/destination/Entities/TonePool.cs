using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("tone_pool")]
public class TonePool : BaseType<int>
{
    #region Properties
    [Column("owner_id")]
    public int OwnerId { get; set; }

    public User? Owner { get; set; }


    [Column("is_public")]
    public bool IsPublic { get; set; } = false;

    public List<ContentTonePool> ContentTonePools { get; } = new List<ContentTonePool>();
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