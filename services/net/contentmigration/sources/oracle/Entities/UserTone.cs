using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// UsersTones class, provides an entity to store News Item Tone records in the database.
/// </summary>
public class UserTone
{
    /// <summary>
    /// get/set.
    /// </summary>
    public long ItemRSN { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    public long UserRSN { get; set; }

    /// <summary>
    /// get/set.
    /// </summary>
    public long ToneValue { get; set; }
}
