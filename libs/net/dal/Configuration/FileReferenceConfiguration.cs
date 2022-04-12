namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class FileReferenceConfiguration : AuditColumnsConfiguration<FileReference>
{
    public override void Configure(EntityTypeBuilder<FileReference> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ContentId).IsRequired();
        builder.Property(m => m.MimeType).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Path).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Size).IsRequired();
        builder.Property(m => m.RunningTime).IsRequired();

        builder.HasOne(m => m.Content).WithMany(m => m.FileReferences).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
