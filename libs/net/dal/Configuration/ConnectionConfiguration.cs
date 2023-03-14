namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class ConnectionConfiguration : BaseTypeConfiguration<Connection, int>
{
    public override void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.ConnectionType).IsRequired().HasDefaultValue(ConnectionType.LocalVolume);
        builder.Property(m => m.Configuration).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(m => m.IsReadOnly).IsRequired();

        builder.HasIndex(m => m.Name, "IX_name").IsUnique();

        base.Configure(builder);
    }
}
