namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class SourceActionConfiguration : BaseTypeConfiguration<SourceAction, int>
{
    public override void Configure(EntityTypeBuilder<SourceAction> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();

        base.Configure(builder);
    }
}
