using System;

namespace SilverlandCRM.Domain.Entities
{
    public class Lead
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string LeadDate { get; set; } = null!;
        public string LeadTime { get; set; } = null!;
        public string LeadName { get; set; } = null!;
        public string MobileNumber { get; set; } = null!;
        public string? Email { get; set; }
        public string EnquiredOn { get; set; } = null!;
        public string Response { get; set; } = "New";
        public string? ResponseDetails { get; set; }
        
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }

        public string LeadSource { get; set; } = "Manual";
        public string? SourceLeadId { get; set; }
        public string? SourceListingId { get; set; }

        public Guid? AssignedEmployeeId { get; set; }
        public User? AssignedEmployee { get; set; }

        public Guid CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ImportedAt { get; set; }
        public string? ImportBatchId { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
