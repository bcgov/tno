using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

[Cache("minister", "lookups")]
[Table("minister")]

public class Minister : BaseType<int>
{

    #region Constructors
    protected Minister() { }

    public Minister(string name) : base(name)
    {
    }
    #endregion
}