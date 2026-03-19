using FieldOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Infrastructure.Persistence.Configurations;

public class SlaConfigConfiguration : IEntityTypeConfiguration<SlaConfig>
{
    public void Configure(EntityTypeBuilder<SlaConfig> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.JobType).IsRequired().HasMaxLength(100);

        builder.HasOne(s => s.Client)
            .WithMany(c => c.SlaConfigs)
            .HasForeignKey(s => s.ClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
