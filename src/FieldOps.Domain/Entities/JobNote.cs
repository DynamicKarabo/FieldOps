using FieldOps.Domain.Enums;

namespace FieldOps.Domain.Entities;

public class JobNote
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Guid ClientId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public JobAuthorType AuthorType { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AttachmentsJson { get; set; } = "[]";
    public bool IsOfflineSynced { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation
    public Job Job { get; set; } = null!;
    public Client Client { get; set; } = null!;
}
