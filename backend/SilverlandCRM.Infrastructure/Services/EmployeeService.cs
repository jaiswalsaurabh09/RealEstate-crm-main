using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Domain.Entities;
using SilverlandCRM.Domain.Enums;
using SilverlandCRM.Infrastructure.Data;
using SilverlandCRM.Infrastructure.Identity;

namespace SilverlandCRM.Infrastructure.Services;

public interface IEmployeeService
{
    Task<PagedResult<EmployeeDto>> GetEmployeesAsync(int page, int pageSize, string? search);
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id);
    Task<EmployeeDto> CreateEmployeeAsync(CreateEmployeeDto dto, Guid adminId, string adminName);
    Task<EmployeeDto?> UpdateEmployeeAsync(Guid id, UpdateEmployeeDto dto, Guid adminId, string adminName);
    Task<bool> DeleteEmployeeAsync(Guid id, Guid adminId, string adminName);
    Task ToggleLoginAsync(Guid id, bool enabled, Guid adminId, string adminName);
    Task ResetPasswordAsync(Guid id, string newPassword, Guid adminId, string adminName);
}

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _db;

    public EmployeeService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<EmployeeDto>> GetEmployeesAsync(
        int page,
        int pageSize,
        string? search)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Users
            .AsNoTracking()
            .Where(u =>
                u.Role == UserRole.Employee &&
                !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(u =>
                u.Name.Contains(search) ||
                u.Email.Contains(search) ||
                u.Mobile.Contains(search) ||
                (u.EmployeeCode != null && u.EmployeeCode.Contains(search)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new EmployeeDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Mobile = u.Mobile,
                EmployeeCode = u.EmployeeCode,
                IsLoginEnabled = u.IsLoginEnabled,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<EmployeeDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(Guid id)
    {
        return await _db.Users
            .AsNoTracking()
            .Where(u =>
                u.Id == id &&
                u.Role == UserRole.Employee &&
                !u.IsDeleted)
            .Select(u => new EmployeeDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Mobile = u.Mobile,
                EmployeeCode = u.EmployeeCode,
                IsLoginEnabled = u.IsLoginEnabled,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(
        CreateEmployeeDto dto,
        Guid adminId,
        string adminName)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email && !u.IsDeleted))
            throw new InvalidOperationException("An employee with this email already exists.");

        if (await _db.Users.AnyAsync(u => u.Mobile == dto.Mobile && !u.IsDeleted))
            throw new InvalidOperationException("An employee with this mobile number already exists.");

        var employee = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Mobile = dto.Mobile,
            EmployeeCode = dto.EmployeeCode,
            PasswordHash = PasswordHasher.HashPassword(dto.Password),
            Role = UserRole.Employee,
            IsLoginEnabled = true,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(employee);

        _db.EmployeeAudits.Add(new EmployeeAudit
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            Action = "Created",
            ChangedByUserId = adminId,
            ChangedByUserName = adminName,
            ChangedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();

        return ToDto(employee);
    }

    public async Task<EmployeeDto?> UpdateEmployeeAsync(
        Guid id,
        UpdateEmployeeDto dto,
        Guid adminId,
        string adminName)
    {
        var employee = await _db.Users.FirstOrDefaultAsync(u =>
            u.Id == id &&
            u.Role == UserRole.Employee &&
            !u.IsDeleted);

        if (employee == null)
            return null;

        employee.Name = dto.Name;
        employee.Email = dto.Email;
        employee.Mobile = dto.Mobile;
        employee.EmployeeCode = dto.EmployeeCode;
        employee.UpdatedAt = DateTime.UtcNow;

        _db.EmployeeAudits.Add(new EmployeeAudit
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            Action = "Updated",
            ChangedByUserId = adminId,
            ChangedByUserName = adminName,
            ChangedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();

        return ToDto(employee);
    }

    public async Task<bool> DeleteEmployeeAsync(
        Guid id,
        Guid adminId,
        string adminName)
    {
        var employee = await _db.Users.FirstOrDefaultAsync(u =>
            u.Id == id &&
            u.Role == UserRole.Employee &&
            !u.IsDeleted);

        if (employee == null)
            return false;

        var hasActiveLeads = await _db.Leads.AnyAsync(l =>
            l.AssignedEmployeeId == id &&
            !l.IsDeleted);

        if (hasActiveLeads)
            return false;

        employee.IsDeleted = true;
        employee.IsActive = false;
        employee.IsLoginEnabled = false;
        employee.DeletedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;

        _db.EmployeeAudits.Add(new EmployeeAudit
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            Action = "Deleted",
            ChangedByUserId = adminId,
            ChangedByUserName = adminName,
            ChangedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task ToggleLoginAsync(
        Guid id,
        bool enabled,
        Guid adminId,
        string adminName)
    {
        var employee = await GetActiveEmployee(id);

        if (employee == null)
            throw new KeyNotFoundException("Employee not found.");

        // Do not create unnecessary audit records when
        // the requested login state is already applied.
        if (employee.IsLoginEnabled == enabled)
            return;

        employee.IsLoginEnabled = enabled;
        employee.UpdatedAt = DateTime.UtcNow;

        _db.EmployeeAudits.Add(new EmployeeAudit
        {
            EmployeeId = employee.Id,
            Action = enabled ? "LoginEnabled" : "LoginDisabled",
            ChangedByUserId = adminId,
            ChangedByUserName = adminName,
            ChangedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();
    }

    public async Task ResetPasswordAsync(
        Guid id,
        string newPassword,
        Guid adminId,
        string adminName)
    {
        var employee = await GetActiveEmployee(id);

        if (employee == null)
            throw new KeyNotFoundException("Employee not found.");

        employee.PasswordHash = PasswordHasher.HashPassword(newPassword);
        employee.UpdatedAt = DateTime.UtcNow;

        _db.EmployeeAudits.Add(new EmployeeAudit
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            Action = "PasswordReset",
            ChangedByUserId = adminId,
            ChangedByUserName = adminName,
            ChangedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();
    }

    private async Task<User?> GetActiveEmployee(Guid id)
    {
        return await _db.Users.FirstOrDefaultAsync(u =>
            u.Id == id &&
            u.Role == UserRole.Employee &&
            !u.IsDeleted);
    }

    private static EmployeeDto ToDto(User user)
    {
        return new EmployeeDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Mobile = user.Mobile,
            EmployeeCode = user.EmployeeCode,
            IsLoginEnabled = user.IsLoginEnabled,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
