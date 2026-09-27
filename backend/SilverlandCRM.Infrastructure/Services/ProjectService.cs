using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Domain.Entities;
using SilverlandCRM.Infrastructure.Data;

namespace SilverlandCRM.Infrastructure.Services;

public interface IProjectService
{
    Task<List<ProjectDto>> GetProjectsAsync(bool includeInactive = false);
    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, Guid adminId, string adminName);
    Task<ProjectDto?> UpdateProjectAsync(Guid id, UpdateProjectDto dto, Guid adminId, string adminName);
    Task<bool> DeleteProjectAsync(Guid id, Guid adminId, string adminName);
    Task<bool> SetActiveAsync(Guid id, bool active, Guid adminId, string adminName);
}

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _db;

    public ProjectService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProjectDto>> GetProjectsAsync(bool includeInactive = false)
    {
        var query = _db.Projects
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new ProjectDto { Id = p.Id, Name = p.Name, Location = p.Location, Description = p.Description, IsActive = p.IsActive, CreatedAt = p.CreatedAt })
            .ToListAsync();
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(Guid id)
    {
        return await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == id && !p.IsDeleted)
            .Select(p => new ProjectDto { Id = p.Id, Name = p.Name, Location = p.Location, Description = p.Description, IsActive = p.IsActive, CreatedAt = p.CreatedAt })
            .FirstOrDefaultAsync();
    }

    public async Task<ProjectDto> CreateProjectAsync(
        CreateProjectDto dto,
        Guid adminId,
        string adminName)
    {
        var name = dto.Name.Trim();

        if (await _db.Projects.AnyAsync(p =>
            p.Name.ToLower() == name.ToLower() &&
            !p.IsDeleted))
        {
            throw new InvalidOperationException(
                "A project with this name already exists.");
        }

        var project = new Project
        {
            Name = name,
            Location = dto.Location.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Projects.Add(project);

        _db.ProjectAudits.Add(new ProjectAudit
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Action = "Created",
            ChangedByUserId = adminId,
            ChangedByUserName = adminName,
            ChangedAt = DateTime.UtcNow,
            NewValue = project.Name
        });

        await _db.SaveChangesAsync();

        return ToDto(project);
    }

    public async Task<ProjectDto?> UpdateProjectAsync(
        Guid id,
        UpdateProjectDto dto,
        Guid adminId,
        string adminName)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p =>
            p.Id == id && !p.IsDeleted);

        if (project == null)
            return null;

        var duplicate = await _db.Projects.AnyAsync(p =>
            p.Id != id &&
            p.Name.ToLower() == dto.Name.Trim().ToLower() &&
            !p.IsDeleted);

        if (duplicate)
            throw new InvalidOperationException(
                "A project with this name already exists.");

        var oldValue = $"{project.Name} | {project.Location}";

        project.Name = dto.Name.Trim();
        project.Location = dto.Location.Trim();
        project.Description = dto.Description?.Trim();
        project.UpdatedAt = DateTime.UtcNow;

        _db.ProjectAudits.Add(new ProjectAudit
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Action = "Updated",
            ChangedByUserId = adminId,
            ChangedByUserName = adminName,
            ChangedAt = DateTime.UtcNow,
            OldValue = oldValue,
            NewValue = $"{project.Name} | {project.Location}"
        });

        await _db.SaveChangesAsync();

        return ToDto(project);
    }

    public async Task<bool> DeleteProjectAsync(
        Guid id,
        Guid adminId,
        string adminName)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p =>
            p.Id == id && !p.IsDeleted);

        if (project == null)
            return false;

        var hasLeads = await _db.Leads.AnyAsync(l =>
            l.ProjectId == id && !l.IsDeleted);

        if (hasLeads)
            throw new InvalidOperationException(
                "Cannot remove a project that has assigned leads.");

        project.IsDeleted = true;
        project.IsActive = false;
        project.DeletedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        _db.ProjectAudits.Add(new ProjectAudit
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Action = "Deleted",
            ChangedByUserId = adminId,
            ChangedByUserName = adminName,
            ChangedAt = DateTime.UtcNow,
            OldValue = "Active",
            NewValue = "Deleted"
        });

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetActiveAsync(
        Guid id,
        bool active,
        Guid adminId,
        string adminName)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p =>
            p.Id == id && !p.IsDeleted);

        if (project == null)
            return false;

        project.IsActive = active;
        project.UpdatedAt = DateTime.UtcNow;

        _db.ProjectAudits.Add(new ProjectAudit
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            Action = active ? "Activated" : "Deactivated",
            ChangedByUserId = adminId,
            ChangedByUserName = adminName,
            ChangedAt = DateTime.UtcNow,
            OldValue = active ? "Inactive" : "Active",
            NewValue = active ? "Active" : "Inactive"
        });

        await _db.SaveChangesAsync();

        return true;
    }

    private static ProjectDto ToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Location = project.Location,
            Description = project.Description,
            IsActive = project.IsActive,
            CreatedAt = project.CreatedAt
        };
    }


}
