using FieldOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Infrastructure.Persistence.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Reference).IsRequired().HasMaxLength(50);
        builder.Property(j => j.JobType).IsRequired().HasMaxLength(100);
        builder.Property(j => j.Title).IsRequired().HasMaxLength(200);

        builder.OwnsOne(j => j.SiteAddress, a =>
        {
            a.Property(p => p.Street).HasMaxLength(200);
            a.Property(p => p.City).HasMaxLength(100);
            a.Property(p => p.Province).HasMaxLength(100);
            a.Property(p => p.PostalCode).HasMaxLength(20);
            a.Property(p => p.Country).HasMaxLength(100);
        });

        builder.OwnsOne(j => j.SlaDeadlines);

        builder.HasOne(j => j.Client)
            .WithMany(c => c.Jobs)
            .HasForeignKey(j => j.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(j => j.AssignedTechnician)
            .WithMany(t => t.AssignedJobs)
            .HasForeignKey(j => j.AssignedTechnicianId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(j => j.SlaConfig)
            .WithMany()
            .HasForeignKey(j => j.SlaConfigId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
