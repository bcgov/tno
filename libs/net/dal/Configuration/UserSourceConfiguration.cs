namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserSourceConfiguration : AuditColumnsConfiguration<UserSource>
{
    public override void Configure(EntityTypeBuilder<UserSource> builder)
    {
        builder.HasKey(m => new { m.UserId, m.SourceId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.SourceId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.User).WithMany(m => m.SourcesManyToMany).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Source).WithMany(m => m.UsersManyToMany).HasForeignKey(m => m.SourceId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
