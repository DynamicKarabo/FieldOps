using FieldOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Infrastructure.Persistence.Configurations;

public class TechnicianConfiguration : IEntityTypeConfiguration<Technician>
{
    public void Configure(EntityTypeBuilder<Technician> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.EmployeeNumber).IsRequired().HasMaxLength(50);
        builder.Property(t => t.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.LastName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Email).IsRequired().HasMaxLength(256);
        builder.Property(t => t.Phone).IsRequired().HasMaxLength(50);

        builder.HasOne(t => t.Client)
            .WithMany(c => c.Technicians)
            .HasForeignKey(t => t.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Region)
            .WithMany(r => r.Technicians)
            .HasForeignKey(t => t.RegionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.CurrentJob)
            .WithOne()
            .HasForeignKey<Technician>(t => t.CurrentJobId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
