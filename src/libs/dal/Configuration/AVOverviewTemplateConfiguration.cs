namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AVOverViewTemplateConfiguration : AuditColumnsConfiguration<AVOverviewTemplate>
{
    public override void Configure(EntityTypeBuilder<AVOverviewTemplate> builder)
    {
        builder.HasKey(m => m.TemplateType);
        builder.Property(m => m.TemplateType).IsRequired().ValueGeneratedNever();
        builder.Property(m => m.TemplateId).IsRequired();

        builder.HasOne(m => m.Template).WithMany().HasForeignKey(m => m.TemplateId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(m => m.Subscribers).WithMany(m => m.AVOverviewSubscriptions).UsingEntity<UserAVOverview>();

        base.Configure(builder);
    }
}
