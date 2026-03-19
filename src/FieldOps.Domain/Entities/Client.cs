using FieldOps.Domain.Enums;

namespace FieldOps.Domain.Entities;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContractReference { get; set; } = string.Empty;
    public string DefaultSlaConfigJson { get; set; } = "{}";
    public ClientStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public ICollection<SlaConfig> SlaConfigs { get; set; } = new List<SlaConfig>();
    public ICollection<Region> Regions { get; set; } = new List<Region>();
    public ICollection<Technician> Technicians { get; set; } = new List<Technician>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
