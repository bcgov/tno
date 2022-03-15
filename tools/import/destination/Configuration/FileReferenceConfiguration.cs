namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class FileReferenceConfiguration : IEntityTypeConfiguration<FileReference>
{
    public virtual void Configure(EntityTypeBuilder<FileReference> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ContentId).IsRequired();
        builder.Property(m => m.MimeType).IsRequired();
        builder.Property(m => m.Path).IsRequired();
        builder.Property(m => m.Size).IsRequired();
        builder.Property(m => m.RunningTime).IsRequired();

        builder.HasOne(m => m.Content).WithMany(m => m.FileReferences).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);
    }
}