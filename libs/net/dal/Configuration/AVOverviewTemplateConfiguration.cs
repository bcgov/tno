namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class AVOverViewTemplateConfiguration : BaseTypeConfiguration<AVOverviewTemplate, int>
{
        public override void Configure(EntityTypeBuilder<AVOverviewTemplate> builder)
        {
            builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(m => m.TemplateType).IsRequired();
            builder.Property(m => m.Template).IsRequired().HasColumnType("text");;
            builder.HasIndex(m => m.Name).IsUnique();

            base.Configure(builder);
        }
}