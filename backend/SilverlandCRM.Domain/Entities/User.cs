using System;
using SilverlandCRM.Domain.Enums;

namespace SilverlandCRM.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Mobile { get; set; } = null!;
    public string? EmployeeCode { get; set; }
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.Employee;
    public bool IsLoginEnabled { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}