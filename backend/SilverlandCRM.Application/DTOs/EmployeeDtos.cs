// EmployeeDtos.cs
namespace SilverlandCRM.Application.DTOs;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Mobile { get; set; } = null!;
    public string? EmployeeCode { get; set; }
    public string Role { get; set; } = null!;
    public bool IsLoginEnabled { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Mobile { get; set; } = null!;
    public string? EmployeeCode { get; set; }
    public string Password { get; set; } = null!;
    public string Role { get; set; } = "Employee";
}

public class UpdateEmployeeDto
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Mobile { get; set; } = null!;
    public string? EmployeeCode { get; set; }
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = null!;
}