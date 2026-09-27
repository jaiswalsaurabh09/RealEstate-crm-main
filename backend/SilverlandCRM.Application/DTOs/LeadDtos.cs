// LeadDtos.cs
namespace SilverlandCRM.Application.DTOs;

public class LeadDto
{
    public Guid Id { get; set; }
    public string LeadDate { get; set; } = null!;
    public string LeadTime { get; set; } = null!;
    public string LeadName { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string EnquiredOn { get; set; } = null!;
    public string Response { get; set; } = null!;
    public string? ResponseDetails { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string LeadSource { get; set; } = null!;
    public string? SourceLeadId { get; set; }
    public string? SourceListingId { get; set; }
    public Guid? AssignedEmployeeId { get; set; }
    public string? AssignedEmployeeName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ImportedAt { get; set; }
    public string? ImportBatchId { get; set; }
}

public class CreateLeadDto
{
    public string LeadName { get; set; } = null!;
    public string MobileNumber { get; set; } = null!;
    public string? Email { get; set; }
    public string EnquiredOn { get; set; } = null!;
    public string Response { get; set; } = "New";
    public string? ResponseDetails { get; set; }
    public Guid? ProjectId { get; set; }
    public string LeadSource { get; set; } = "Manual";
    public string? SourceLeadId { get; set; }
    public string? SourceListingId { get; set; }
    public Guid? AssignedEmployeeId { get; set; }
}

public class UpdateLeadDto : CreateLeadDto { }

public class AssignLeadDto
{
    public Guid? AssignedEmployeeId { get; set; }
}

public class BulkAssignLeadDto
{
    public Guid AssignedEmployeeId { get; set; }
}

public class LeadAuditDto
{
    public Guid Id { get; set; }
    public Guid LeadId { get; set; }
    public string MobileNumber { get; set; } = null!;
    public string LeadName { get; set; } = null!;
    public string ChangedByUserName { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
    public string ColumnName { get; set; } = null!;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string Action { get; set; } = null!;
}

public class ImportSummaryDto
{
    public string BatchId { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public int TotalRows { get; set; }
    public int CreatedRows { get; set; }
    public int UpdatedRows { get; set; }
    public int DuplicateRows { get; set; }
    public int FailedRows { get; set; }
    public List<string> Errors { get; set; } = new();
}
