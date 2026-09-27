using System.Linq;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Infrastructure.Data;

namespace SilverlandCRM.Infrastructure.Services;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, string userRole);
}

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;

    public DashboardService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid userId, string userRole)
    {
        var leadsQuery = _db.Leads.Where(l => !l.IsDeleted);

        if (userRole == "Employee")
        {
            leadsQuery = leadsQuery.Where(l => l.AssignedEmployeeId == userId);
        }

        var totalLeads = leadsQuery.Count();
        var newLeads = leadsQuery.Count(l => l.Response == "New");
        var followUpLeads = leadsQuery.Count(l => l.Response == "Call Back" || l.Response == "Contacted");
        var convertedLeads = leadsQuery.Count(l => l.Response == "Booked" || l.Response == "Site Visit Completed");
        var lostLeads = leadsQuery.Count(l => l.Response == "Lost" || l.Response == "Not Interested");

        var totalProjects = _db.Projects.Count(p => !p.IsDeleted);
        var activeProjects = _db.Projects.Count(p => p.IsActive && !p.IsDeleted);
        var totalEmployees = _db.Users.Count(u => u.Role == Domain.Enums.UserRole.Employee && !u.IsDeleted);
        var assignedCount = _db.Leads.Count(l => l.AssignedEmployeeId == userId && !l.IsDeleted);

        var recentLeads = leadsQuery
            .OrderByDescending(l => l.CreatedAt)
            .Take(5)
            .Select(l => new LeadDto
            {
                Id = l.Id,
                LeadName = l.LeadName,
                MobileNumber = l.MobileNumber,
                Response = l.Response,
                LeadSource = l.LeadSource,
                CreatedAt = l.CreatedAt
            })
            .ToList();

        var recentAudits = _db.LeadAudits
            .OrderByDescending(a => a.ChangedAt)
            .Take(5)
            .Select(a => new LeadAuditDto
            {
                Id = a.Id,
                LeadId = a.LeadId,
                MobileNumber = a.MobileNumber,
                LeadName = a.LeadName,
                ChangedByUserName = a.ChangedByUserName,
                ChangedAt = a.ChangedAt,
                ColumnName = a.ColumnName,
                OldValue = a.OldValue,
                NewValue = a.NewValue,
                Action = a.Action
            })
            .ToList();

        return await Task.FromResult(new DashboardStatsDto
        {
            TotalLeads = totalLeads,
            NewLeads = newLeads,
            FollowUpLeads = followUpLeads,
            ConvertedLeads = convertedLeads,
            LostLeads = lostLeads,
            TotalProjects = totalProjects,
            ActiveProjects = activeProjects,
            TotalEmployees = totalEmployees,
            AssignedLeadsToCurrentEmployee = assignedCount,
            RecentLeads = recentLeads,
            RecentAudits = recentAudits
        });
    }
}