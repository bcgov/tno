namespace TNO.DAL.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Entities;

public class WorkOrderConfiguration : AuditColumnsConfiguration<WorkOrder>
{
    public override void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Status).IsRequired();
        builder.Property(m => m.WorkType).IsRequired();

        builder.Property(m => m.RequestorId);
        builder.Property(m => m.AssignedId);
        builder.Property(m => m.Description).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Note).IsRequired().HasColumnType("text");
        builder.Property(m => m.Configuration).IsRequired().HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        builder.HasOne(m => m.Requestor).WithMany(m => m.WorkOrderRequests).HasForeignKey(m => m.RequestorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Assigned).WithMany(m => m.WorkOrdersAssigned).HasForeignKey(m => m.AssignedId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(m => m.Content).WithMany(m => m.WorkOrders).HasForeignKey(m => m.ContentId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.WorkType, m.Status, m.CreatedOn, m.RequestorId, m.AssignedId }, "IX_work_order");

        base.Configure(builder);
    }
}
