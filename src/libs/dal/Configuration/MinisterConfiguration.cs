namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class MinisterConfiguration : BaseTypeConfiguration<Minister, int>
{
    public override void Configure(EntityTypeBuilder<Minister> builder)
    {
        builder.Property(m => m.OrganizationId);
        builder.Property(m => m.Position).IsRequired().HasMaxLength(250).HasDefaultValueSql("''");
        builder.Property(m => m.Aliases).IsRequired().HasMaxLength(250).HasDefaultValueSql("''");

        builder.HasOne(m => m.Organization).WithMany(m => m.Ministers).HasForeignKey(m => m.OrganizationId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.Name).IsUnique();

        base.Configure(builder);
    }
}
