namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class FolderConfiguration : BaseTypeConfiguration<Folder, int>
{
    public override void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.OwnerId);
        builder.Property(m => m.FilterId);
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.Owner).WithMany(m => m.Folders).HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Filter).WithMany(m => m.Folders).HasForeignKey(m => m.FilterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.Content).WithMany(m => m.Folders).UsingEntity<FolderContent>();

        builder.HasIndex(m => new { m.OwnerId, m.Name }).IsUnique();

        base.Configure(builder);
    }
}
