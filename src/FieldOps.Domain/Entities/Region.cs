namespace FieldOps.Domain.Entities;

public class Region
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BoundariesGeoJson { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation
    public Client Client { get; set; } = null!;
    public ICollection<Technician> Technicians { get; set; } = new List<Technician>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
}
