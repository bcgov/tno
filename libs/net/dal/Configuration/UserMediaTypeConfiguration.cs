namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserMediaTypeConfiguration : AuditColumnsConfiguration<UserMediaType>
{
    public override void Configure(EntityTypeBuilder<UserMediaType> builder)
    {
        builder.HasKey(m => new { m.UserId, m.MediaTypeId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.MediaTypeId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.User).WithMany(m => m.MediaTypesManyToMany).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.MediaType).WithMany(m => m.UsersManyToMany).HasForeignKey(m => m.MediaTypeId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
