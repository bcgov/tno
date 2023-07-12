namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class FilterConfiguration : BaseTypeConfiguration<Filter, int>
{
    public override void Configure(EntityTypeBuilder<Filter> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.OwnerId);
        builder.Property(m => m.Query).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(m => m.Settings).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.Owner).WithMany(m => m.Filters).HasForeignKey(m => m.OwnerId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.OwnerId, m.Name }).IsUnique();

        base.Configure(builder);
    }
}
