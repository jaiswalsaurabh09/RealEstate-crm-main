// DashboardDtos.cs
namespace SilverlandCRM.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalLeads { get; set; }
    public int NewLeads { get; set; }
    public int FollowUpLeads { get; set; }
    public int ConvertedLeads { get; set; }
    public int LostLeads { get; set; }
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalEmployees { get; set; }
    public int AssignedLeadsToCurrentEmployee { get; set; }
    public List<LeadDto> RecentLeads { get; set; } = new();
    public List<LeadAuditDto> RecentAudits { get; set; } = new();
}