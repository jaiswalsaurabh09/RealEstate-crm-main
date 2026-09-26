using System;

namespace SilverlandCRM.Domain.Entities;

public class IntegrationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Provider { get; set; } = null!;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    public string? SourceLeadId { get; set; }
    public string MobileNumber { get; set; } = null!;
    public string Status { get; set; } = "RECEIVED";
    public string? ErrorMessage { get; set; }
    public string RawPayload { get; set; } = null!;
    public Guid? LeadId { get; set; }
    public long ProcessingTimeMs { get; set; }
}