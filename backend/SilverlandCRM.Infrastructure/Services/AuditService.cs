using System;
using System.Threading.Tasks;
using SilverlandCRM.Domain.Entities;
using SilverlandCRM.Infrastructure.Data;

namespace SilverlandCRM.Infrastructure.Services
{
    public class AuditService
    {
        private readonly ApplicationDbContext _db;

        public AuditService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task RecordLeadFieldChangeAsync(
            Guid leadId,
            string mobileNumber,
            string leadName,
            Guid userId,
            string userName,
            string columnName,
            string? oldValue,
            string? newValue,
            string action = "UPDATE")
        {
            if (oldValue == newValue) return;

            var auditEntry = new LeadAudit
            {
                Id = Guid.NewGuid(),
                LeadId = leadId,
                MobileNumber = mobileNumber,
                LeadName = leadName,
                ChangedByUserId = userId,
                ChangedByUserName = userName,
                ChangedAt = DateTime.UtcNow,
                ColumnName = columnName,
                OldValue = oldValue ?? "-",
                NewValue = newValue ?? "-",
                Action = action
            };

            await _db.LeadAudits.AddAsync(auditEntry);
            await _db.SaveChangesAsync();
        }
    }
}
