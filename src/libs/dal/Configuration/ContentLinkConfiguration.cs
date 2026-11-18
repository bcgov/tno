namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ContentLinkConfiguration : AuditColumnsConfiguration<ContentLink>
{
    public override void Configure(EntityTypeBuilder<ContentLink> builder)
    {
        builder.HasKey(m => new { m.ContentId, m.LinkId });
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.LinkId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.Content).WithMany(m => m.Links).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Link).WithMany().HasForeignKey(m => m.LinkId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
