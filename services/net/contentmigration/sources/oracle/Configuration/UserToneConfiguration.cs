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
        builder.HasKey(x => new { x.ItemRSN, x.UserRSN });

        builder.HasOne(m => m.NewsItem).WithMany(m => m.Tones).HasForeignKey(m => m.ItemRSN);
        builder.HasOne(m => m.HNewsItem).WithMany(m => m.Tones).HasForeignKey(m => m.ItemRSN);
        builder.HasOne(m => m.AllNewsItem).WithMany(m => m.Tones).HasForeignKey(m => m.ItemRSN);
    }
}
