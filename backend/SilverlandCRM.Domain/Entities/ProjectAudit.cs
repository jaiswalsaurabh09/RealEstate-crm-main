using System;

namespace SilverlandCRM.Domain.Entities;

public class ProjectAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public Guid ChangedByUserId { get; set; }
    public string ChangedByUserName { get; set; } = null!;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
}
