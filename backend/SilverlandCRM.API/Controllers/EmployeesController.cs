// EmployeesController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Infrastructure.Services;

namespace SilverlandCRM.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployees([FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? search = null)
    {
        var result = await _employeeService.GetEmployeesAsync(page, pageSize, search);
        return Ok(ApiResponse<PagedResult<EmployeeDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEmployeeById(Guid id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound(ApiResponse<string>.Fail("Employee not found"));
        return Ok(ApiResponse<EmployeeDto>.Ok(employee));
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var adminName = User.FindFirstValue(ClaimTypes.Name)!;

        var employee = await _employeeService.CreateEmployeeAsync(dto, adminId, adminName);
        return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, ApiResponse<EmployeeDto>.Ok(employee, "Employee created"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeDto dto)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var adminName = User.FindFirstValue(ClaimTypes.Name)!;

        var employee = await _employeeService.UpdateEmployeeAsync(id, dto, adminId, adminName);
        if (employee == null) return NotFound(ApiResponse<string>.Fail("Employee not found"));
        return Ok(ApiResponse<EmployeeDto>.Ok(employee, "Employee updated"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var adminName = User.FindFirstValue(ClaimTypes.Name)!;

        var result = await _employeeService.DeleteEmployeeAsync(id, adminId, adminName);
        if (!result) return BadRequest(ApiResponse<string>.Fail("Cannot remove employee with assigned active leads. Reassign leads first."));
        return Ok(ApiResponse<string>.Ok("Employee soft-deleted successfully"));
    }

    [HttpPost("{id:guid}/disable-login")]
    public async Task<IActionResult> DisableLogin(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var adminName = User.FindFirstValue(ClaimTypes.Name)!;

        await _employeeService.ToggleLoginAsync(id, false, adminId, adminName);
        return Ok(ApiResponse<string>.Ok("Employee login disabled"));
    }

    [HttpPost("{id:guid}/enable-login")]
    public async Task<IActionResult> EnableLogin(Guid id)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var adminName = User.FindFirstValue(ClaimTypes.Name)!;

        await _employeeService.ToggleLoginAsync(id, true, adminId, adminName);
        return Ok(ApiResponse<string>.Ok("Employee login enabled"));
    }

    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, [FromBody] ResetPasswordDto dto)
    {
        var adminId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var adminName = User.FindFirstValue(ClaimTypes.Name)!;

        await _employeeService.ResetPasswordAsync(id, dto.NewPassword, adminId, adminName);
        return Ok(ApiResponse<string>.Ok("Employee password reset successfully"));
    }
}