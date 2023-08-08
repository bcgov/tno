using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TNO.Services.ContentMigration.Sources.Oracle;

/// <summary>
/// UsersTonesConfiguration class, can be used for configuration of UsersTones Entity
/// </summary>
public class UserToneConfiguration : IEntityTypeConfiguration<UserTone>
{
    /// <summary>
    /// Configure the Entity.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public virtual void Configure(EntityTypeBuilder<UserTone> builder)
    {
        builder.ToTable("USERS_TONES", "TNO");
        builder.HasKey(x => new { x.ItemRSN,x.UserRSN});

        builder.Property(e => e.ItemRSN).HasColumnName("ITEM_RSN");
        builder.Property(e => e.UserRSN).HasColumnName("USER_RSN");
        builder.Property(e => e.ToneValue).HasColumnName("TONE");
    }
}
