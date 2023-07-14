namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportConfiguration : BaseTypeConfiguration<Report, int>
{
    public override void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.OwnerId);
        builder.Property(m => m.TemplateId).IsRequired();
        builder.Property(m => m.IsPublic).IsRequired();
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.Template).WithMany(m => m.Reports).HasForeignKey(m => m.TemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Owner).WithMany(m => m.Reports).HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.Subscribers).WithMany(m => m.ReportSubscriptions).UsingEntity<UserReport>();

        builder.HasIndex(m => new { m.OwnerId, m.Name }).IsUnique();

        base.Configure(builder);
    }
}
