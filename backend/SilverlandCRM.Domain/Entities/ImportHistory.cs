using System;

namespace SilverlandCRM.Domain.Entities;

public class ImportHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string BatchId { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ImportedByUserName { get; set; } = null!;
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    public int TotalRows { get; set; }
    public int CreatedRows { get; set; }
    public int UpdatedRows { get; set; }
    public int DuplicateRows { get; set; }
    public int FailedRows { get; set; }
    public string Status { get; set; } = "Completed";
}