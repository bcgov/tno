namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class UserOrganizationConfiguration : AuditColumnsConfiguration<UserOrganization>
{
    public override void Configure(EntityTypeBuilder<UserOrganization> builder)
    {
        builder.HasKey(m => new { m.UserId, m.OrganizationId });
        builder.Property(m => m.UserId).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.OrganizationId).IsRequired().ValueGeneratedNever();

        builder.HasOne(m => m.User).WithMany(m => m.OrganizationsManyToMany).HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.Organization).WithMany(m => m.UsersManyToMany).HasForeignKey(m => m.OrganizationId).OnDelete(DeleteBehavior.Cascade);

        base.Configure(builder);
    }
}
