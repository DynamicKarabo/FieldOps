using FieldOps.Application.Common.Interfaces;
using FieldOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Infrastructure.Persistence;

public class FieldOpsDbContext : DbContext, IFieldOpsDbContext
{
    public FieldOpsDbContext(DbContextOptions<FieldOpsDbContext> options) : base(options)
    {
    }

    public DbSet<Client> Clients => Set<Client>();
    public DbSet<SlaConfig> SlaConfigs => Set<SlaConfig>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobNote> JobNotes => Set<JobNote>();
    public DbSet<JobStatusEvent> JobStatusEvents => Set<JobStatusEvent>();
    public DbSet<SlaBreachRecord> SlaBreachRecords => Set<SlaBreachRecord>();
    public DbSet<OfflineSyncBatch> OfflineSyncBatches => Set<OfflineSyncBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FieldOpsDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
