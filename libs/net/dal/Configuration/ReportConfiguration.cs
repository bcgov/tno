namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ReportConfiguration : BaseTypeConfiguration<Report, int>
{
    public override void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ReportType).IsRequired();
        builder.Property(m => m.Filter).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasIndex(m => m.Name, "IX_name").IsUnique();

        base.Configure(builder);
    }
}
