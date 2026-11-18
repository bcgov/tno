namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class FolderContentConfiguration : AuditColumnsConfiguration<FolderContent>
{
    public override void Configure(EntityTypeBuilder<FolderContent> builder)
    {
        builder.HasKey(m => new { m.FolderId, m.ContentId });
        builder.Property(m => m.FolderId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.ContentId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.SortOrder).IsRequired();

        builder.HasOne(m => m.Folder).WithMany(m => m.ContentManyToMany).HasForeignKey(m => m.FolderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Content).WithMany(m => m.FoldersManyToMany).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
