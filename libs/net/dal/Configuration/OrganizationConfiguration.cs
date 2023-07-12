namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class OrganizationConfiguration : BaseTypeConfiguration<Organization, int>
{
    public override void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ParentId);

        builder.HasOne(m => m.Parent).WithMany(m => m.Children).HasForeignKey(m => m.ParentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.Users).WithMany(m => m.Organizations).UsingEntity<UserOrganization>();

        builder.HasIndex(m => new { m.ParentId, m.Name }).IsUnique();

        base.Configure(builder);
    }
}
