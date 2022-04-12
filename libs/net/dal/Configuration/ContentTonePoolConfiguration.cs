namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentTonePoolConfiguration : AuditColumnsConfiguration<ContentTonePool>
{
    public override void Configure(EntityTypeBuilder<ContentTonePool> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.TonePoolId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.TonePoolId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.Value).IsRequired();

        builder.HasOne(m => m.Content).WithMany(m => m.TonePoolsManyToMany).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.TonePool).WithMany(m => m.ContentsManyToMany).HasForeignKey(m => m.TonePoolId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
