using FieldOps.Domain.Enums;

namespace FieldOps.Domain.Entities;

public class Technician
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Guid RegionId { get; set; }
    public string SkillsJson { get; set; } = "[]"; // JSON array of strings
    public TechnicianStatus Status { get; set; }
    public decimal? LastKnownLatitude { get; set; }
    public decimal? LastKnownLongitude { get; set; }
    public DateTimeOffset? LastLocationUpdatedAt { get; set; }
    public Guid? CurrentJobId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // Navigation
    public Client Client { get; set; } = null!;
    public Region Region { get; set; } = null!;
    public Job? CurrentJob { get; set; }
    public ICollection<Job> AssignedJobs { get; set; } = new List<Job>();
}
