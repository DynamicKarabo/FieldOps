using FieldOps.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Application.Common.Interfaces;

public interface IFieldOpsDbContext
{
    DbSet<Client> Clients { get; }
    DbSet<SlaConfig> SlaConfigs { get; }
    DbSet<Region> Regions { get; }
    DbSet<Technician> Technicians { get; }
    DbSet<Job> Jobs { get; }
    DbSet<JobNote> JobNotes { get; }
    DbSet<JobStatusEvent> JobStatusEvents { get; }
    DbSet<SlaBreachRecord> SlaBreachRecords { get; }
    DbSet<OfflineSyncBatch> OfflineSyncBatches { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
