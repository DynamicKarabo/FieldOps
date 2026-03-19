using FieldOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Infrastructure.Persistence.Configurations;

public class JobStatusEventConfiguration : IEntityTypeConfiguration<JobStatusEvent>
{
    public void Configure(EntityTypeBuilder<JobStatusEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TriggeredBy).IsRequired().HasMaxLength(100);

        builder.HasOne(e => e.Job)
            .WithMany(j => j.StatusEvents)
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Client)
            .WithMany()
            .HasForeignKey(e => e.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
