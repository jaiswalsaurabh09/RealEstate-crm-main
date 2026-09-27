using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Domain.Entities;
using SilverlandCRM.Domain.Enums;
using SilverlandCRM.Infrastructure.Data;

namespace SilverlandCRM.Infrastructure.Services;

public interface ILeadService
{
    Task<PagedResult<LeadDto>> GetLeadsAsync(
        int page,
        int pageSize,
        string? search,
        Guid? projectId,
        Guid? employeeId,
        string? response,
        string? leadSource,
        Guid userId,
        string role);

    Task<LeadDto?> GetLeadByIdAsync(Guid id, Guid userId, string role);
    Task<List<LeadDto>> SearchAsync(string? mobile, string? name, Guid userId, string role);
    Task<LeadDto> CreateLeadAsync(CreateLeadDto dto, Guid userId, string userName);
    Task<LeadDto?> UpdateLeadAsync(Guid id, UpdateLeadDto dto, Guid userId, string userName, string role);
    Task<bool> DeleteLeadAsync(Guid id, Guid userId, string userName);
    Task<LeadDto?> AssignLeadAsync(Guid id, Guid? employeeId, Guid userId, string userName);
    Task<int> BulkAssignUnassignedLeadsAsync(Guid employeeId, Guid userId, string userName);
    Task<List<LeadAuditDto>> GetAuditAsync(Guid leadId);
}

public class LeadService : ILeadService
{
    private readonly ApplicationDbContext _db;

    public LeadService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<LeadDto>> GetLeadsAsync(
        int page,
        int pageSize,
        string? search,
        Guid? projectId,
        Guid? employeeId,
        string? response,
        string? leadSource,
        Guid userId,
        string role)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Leads
            .AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.AssignedEmployee)
            .Where(x => !x.IsDeleted);

        if (role == UserRole.Employee.ToString())
            query = query.Where(x => x.AssignedEmployeeId == userId);

        if (projectId.HasValue)
            query = query.Where(x => x.ProjectId == projectId.Value);

        if (employeeId.HasValue)
            query = query.Where(x => x.AssignedEmployeeId == employeeId.Value);

        if (!string.IsNullOrWhiteSpace(response))
            query = query.Where(x => x.Response == response);

        if (!string.IsNullOrWhiteSpace(leadSource))
            query = query.Where(x => x.LeadSource == leadSource);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(x =>
                x.LeadName.ToLower().Contains(term) ||
                x.MobileNumber.ToLower().Contains(term) ||
                (x.Email != null && x.Email.ToLower().Contains(term)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new LeadDto
            {
                Id = x.Id,
                LeadDate = x.LeadDate,
                LeadTime = x.LeadTime,
                LeadName = x.LeadName,
                MobileNumber = x.MobileNumber,
                Email = x.Email,
                EnquiredOn = x.EnquiredOn,
                Response = x.Response,
                ResponseDetails = x.ResponseDetails,
                ProjectId = x.ProjectId,
                ProjectName = x.Project != null ? x.Project.Name : null,
                LeadSource = x.LeadSource,
                SourceLeadId = x.SourceLeadId,
                SourceListingId = x.SourceListingId,
                AssignedEmployeeId = x.AssignedEmployeeId,
                AssignedEmployeeName = x.AssignedEmployee != null
                    ? x.AssignedEmployee.Name
                    : null,
                CreatedByUserId = x.CreatedByUserId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                ImportedAt = x.ImportedAt,
                ImportBatchId = x.ImportBatchId
            })
            .ToListAsync();

        return new PagedResult<LeadDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<LeadDto?> GetLeadByIdAsync(Guid id, Guid userId, string role)
    {
        var query = _db.Leads
            .AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.AssignedEmployee)
            .Where(x => x.Id == id && !x.IsDeleted);

        if (role == UserRole.Employee.ToString())
            query = query.Where(x => x.AssignedEmployeeId == userId);

        return await query
            .Select(x => ToDto(x))
            .FirstOrDefaultAsync();
    }

    public async Task<List<LeadDto>> SearchAsync(
        string? mobile,
        string? name,
        Guid userId,
        string role)
    {
        var query = _db.Leads
            .AsNoTracking()
            .Include(x => x.Project)
            .Include(x => x.AssignedEmployee)
            .Where(x => !x.IsDeleted);

        if (role == UserRole.Employee.ToString())
            query = query.Where(x => x.AssignedEmployeeId == userId);

        if (!string.IsNullOrWhiteSpace(mobile))
            query = query.Where(x => x.MobileNumber.Contains(mobile.Trim()));

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.Trim().ToLower();
            query = query.Where(x => x.LeadName.ToLower().Contains(term));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(x => ToDto(x))
            .ToListAsync();
    }

    public async Task<LeadDto> CreateLeadAsync(
        CreateLeadDto dto,
        Guid userId,
        string userName)
    {
        Validate(dto);

        var now = DateTime.UtcNow;

        var lead = new Lead
        {
            LeadDate = now.ToString("yyyy-MM-dd"),
            LeadTime = now.ToString("HH:mm:ss"),
            LeadName = dto.LeadName.Trim(),
            MobileNumber = dto.MobileNumber.Trim(),
            Email = dto.Email?.Trim(),
            EnquiredOn = dto.EnquiredOn.Trim(),
            Response = string.IsNullOrWhiteSpace(dto.Response)
                ? "New"
                : dto.Response.Trim(),
            ResponseDetails = dto.ResponseDetails?.Trim(),
            ProjectId = dto.ProjectId,
            LeadSource = string.IsNullOrWhiteSpace(dto.LeadSource)
                ? "Manual"
                : dto.LeadSource.Trim(),
            SourceLeadId = dto.SourceLeadId?.Trim(),
            SourceListingId = dto.SourceListingId?.Trim(),
            AssignedEmployeeId = dto.AssignedEmployeeId,
            CreatedByUserId = userId,
            CreatedAt = now,
            IsDeleted = false
        };

        _db.Leads.Add(lead);

        _db.LeadAudits.Add(new LeadAudit
        {
            LeadId = lead.Id,
            MobileNumber = lead.MobileNumber,
            LeadName = lead.LeadName,
            ChangedByUserId = userId,
            ChangedByUserName = userName,
            ChangedAt = now,
            ColumnName = "Lead",
            OldValue = null,
            NewValue = lead.LeadName,
            Action = "Created"
        });

        await _db.SaveChangesAsync();

        return await GetLeadByIdAsync(lead.Id, userId, UserRole.Admin.ToString())
            ?? throw new InvalidOperationException("Unable to load created lead.");
    }

    public async Task<LeadDto?> UpdateLeadAsync(
        Guid id,
        UpdateLeadDto dto,
        Guid userId,
        string userName,
        string role)
    {
        Validate(dto);

        var lead = await _db.Leads
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (lead == null)
            return null;

        if (role == UserRole.Employee.ToString() &&
            lead.AssignedEmployeeId != userId)
            return null;

        AddChangeAudit(lead, userId, userName, "LeadName",
            lead.LeadName, dto.LeadName);

        AddChangeAudit(lead, userId, userName, "MobileNumber",
            lead.MobileNumber, dto.MobileNumber);

        AddChangeAudit(lead, userId, userName, "Response",
            lead.Response, dto.Response);

        AddChangeAudit(lead, userId, userName, "ResponseDetails",
            lead.ResponseDetails, dto.ResponseDetails);

        lead.LeadName = dto.LeadName.Trim();
        lead.MobileNumber = dto.MobileNumber.Trim();
        lead.Email = dto.Email?.Trim();
        lead.EnquiredOn = dto.EnquiredOn.Trim();
        lead.Response = dto.Response.Trim();
        lead.ResponseDetails = dto.ResponseDetails?.Trim();
        lead.ProjectId = dto.ProjectId;
        lead.LeadSource = dto.LeadSource.Trim();
        lead.SourceLeadId = dto.SourceLeadId?.Trim();
        lead.SourceListingId = dto.SourceListingId?.Trim();
        lead.AssignedEmployeeId = dto.AssignedEmployeeId;
        lead.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetLeadByIdAsync(id, userId, role);
    }

    public async Task<bool> DeleteLeadAsync(
        Guid id,
        Guid userId,
        string userName)
    {
        var lead = await _db.Leads
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (lead == null)
            return false;

        lead.IsDeleted = true;
        lead.UpdatedAt = DateTime.UtcNow;

        _db.LeadAudits.Add(new LeadAudit
        {
            LeadId = lead.Id,
            MobileNumber = lead.MobileNumber,
            LeadName = lead.LeadName,
            ChangedByUserId = userId,
            ChangedByUserName = userName,
            ChangedAt = DateTime.UtcNow,
            ColumnName = "Lead",
            OldValue = lead.LeadName,
            NewValue = "Deleted",
            Action = "Deleted"
        });

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<LeadDto?> AssignLeadAsync(
        Guid id,
        Guid? employeeId,
        Guid userId,
        string userName)
    {
        var lead = await _db.Leads
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        if (lead == null)
            return null;

        if (employeeId.HasValue)
        {
            var validEmployee = await _db.Users.AnyAsync(x =>
                x.Id == employeeId.Value &&
                x.Role == UserRole.Employee &&
                x.IsActive &&
                x.IsLoginEnabled &&
                !x.IsDeleted);

            if (!validEmployee)
                throw new InvalidOperationException(
                    "Selected employee is not active or login is disabled.");
        }

        string? oldName = null;

        if (lead.AssignedEmployeeId.HasValue)
        {
            oldName = await _db.Users
                .Where(x => x.Id == lead.AssignedEmployeeId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();
        }

        string? newName = null;

        if (employeeId.HasValue)
        {
            newName = await _db.Users
                .Where(x => x.Id == employeeId.Value)
                .Select(x => x.Name)
                .FirstOrDefaultAsync();
        }

        _db.LeadAudits.Add(new LeadAudit
        {
            LeadId = lead.Id,
            MobileNumber = lead.MobileNumber,
            LeadName = lead.LeadName,
            ChangedByUserId = userId,
            ChangedByUserName = userName,
            ChangedAt = DateTime.UtcNow,
            ColumnName = "AssignedEmployee",
            OldValue = oldName,
            NewValue = newName,
            Action = "Assigned"
        });

        lead.AssignedEmployeeId = employeeId;
        lead.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetLeadByIdAsync(
            id,
            userId,
            UserRole.Admin.ToString());
    }

    public async Task<int> BulkAssignUnassignedLeadsAsync(
        Guid employeeId,
        Guid userId,
        string userName)
    {
        var validEmployee = await _db.Users.AnyAsync(x =>
            x.Id == employeeId &&
            x.Role == UserRole.Employee &&
            x.IsActive &&
            x.IsLoginEnabled &&
            !x.IsDeleted);

        if (!validEmployee)
            throw new InvalidOperationException(
                "Selected employee is not active or login is disabled.");

        var employeeName = await _db.Users
            .Where(x => x.Id == employeeId)
            .Select(x => x.Name)
            .FirstAsync();

        var leads = await _db.Leads
            .Where(x =>
                !x.IsDeleted &&
                x.AssignedEmployeeId == null)
            .ToListAsync();

        if (leads.Count == 0)
            return 0;

        var now = DateTime.UtcNow;

        foreach (var lead in leads)
        {
            _db.LeadAudits.Add(new LeadAudit
            {
                LeadId = lead.Id,
                MobileNumber = lead.MobileNumber,
                LeadName = lead.LeadName,
                ChangedByUserId = userId,
                ChangedByUserName = userName,
                ChangedAt = now,
                ColumnName = "AssignedEmployee",
                OldValue = null,
                NewValue = employeeName,
                Action = "Assigned"
            });

            lead.AssignedEmployeeId = employeeId;
            lead.UpdatedAt = now;
        }

        await _db.SaveChangesAsync();

        return leads.Count;
    }

    public async Task<List<LeadAuditDto>> GetAuditAsync(Guid leadId)
    {
        return await _db.LeadAudits
            .AsNoTracking()
            .Where(x => x.LeadId == leadId)
            .OrderByDescending(x => x.ChangedAt)
            .Select(x => new LeadAuditDto
            {
                Id = x.Id,
                LeadId = x.LeadId,
                MobileNumber = x.MobileNumber,
                LeadName = x.LeadName,
                ChangedByUserName = x.ChangedByUserName,
                ChangedAt = x.ChangedAt,
                ColumnName = x.ColumnName,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                Action = x.Action
            })
            .ToListAsync();
    }

    private void AddChangeAudit(
        Lead lead,
        Guid userId,
        string userName,
        string column,
        string? oldValue,
        string? newValue)
    {
        if (oldValue == newValue)
            return;

        _db.LeadAudits.Add(new LeadAudit
        {
            LeadId = lead.Id,
            MobileNumber = lead.MobileNumber,
            LeadName = lead.LeadName,
            ChangedByUserId = userId,
            ChangedByUserName = userName,
            ChangedAt = DateTime.UtcNow,
            ColumnName = column,
            OldValue = oldValue,
            NewValue = newValue,
            Action = "Updated"
        });
    }

    private static LeadDto ToDto(Lead x)
    {
        return new LeadDto
        {
            Id = x.Id,
            LeadDate = x.LeadDate,
            LeadTime = x.LeadTime,
            LeadName = x.LeadName,
            MobileNumber = x.MobileNumber,
            Email = x.Email,
            EnquiredOn = x.EnquiredOn,
            Response = x.Response,
            ResponseDetails = x.ResponseDetails,
            ProjectId = x.ProjectId,
            ProjectName = x.Project?.Name,
            LeadSource = x.LeadSource,
            SourceLeadId = x.SourceLeadId,
            SourceListingId = x.SourceListingId,
            AssignedEmployeeId = x.AssignedEmployeeId,
            AssignedEmployeeName = x.AssignedEmployee?.Name,
            CreatedByUserId = x.CreatedByUserId,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            ImportedAt = x.ImportedAt,
            ImportBatchId = x.ImportBatchId
        };
    }

    private static void Validate(CreateLeadDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.LeadName))
            throw new InvalidOperationException("Lead name is required.");

        if (string.IsNullOrWhiteSpace(dto.MobileNumber))
            throw new InvalidOperationException("Mobile number is required.");

        if (string.IsNullOrWhiteSpace(dto.EnquiredOn))
            throw new InvalidOperationException("Enquired On is required.");
    }
}
