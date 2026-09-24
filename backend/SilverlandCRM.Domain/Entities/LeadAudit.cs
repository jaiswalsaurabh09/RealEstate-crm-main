using System;

namespace SilverlandCRM.Domain.Entities
{
    public class LeadAudit
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid LeadId { get; set; }
        public string MobileNumber { get; set; } = null!;
        public string LeadName { get; set; } = null!;
        public Guid ChangedByUserId { get; set; }
        public string ChangedByUserName { get; set; } = null!;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string ColumnName { get; set; } = null!;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string Action { get; set; } = "UPDATE";
    }
}
