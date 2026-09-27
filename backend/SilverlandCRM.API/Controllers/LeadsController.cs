using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Infrastructure.Services;

namespace SilverlandCRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/leads")]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _service;

    public LeadsController(ILeadService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeads(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? response = null,
        [FromQuery] string? leadSource = null)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        var result = await _service.GetLeadsAsync(
            page, pageSize, search, projectId,
            employeeId, response, leadSource,
            userId, role);

        return Ok(ApiResponse<PagedResult<LeadDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLead(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        var result = await _service.GetLeadByIdAsync(id, userId, role);

        if (result == null)
            return NotFound(ApiResponse<string>.Fail("Lead not found."));

        return Ok(ApiResponse<LeadDto>.Ok(result));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? mobile = null,
        [FromQuery] string? name = null)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        var result = await _service.SearchAsync(
            mobile, name, userId, role);

        return Ok(ApiResponse<List<LeadDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateLeadDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "User";

        try
        {
            var result = await _service.CreateLeadAsync(
                dto, userId, userName);

            return CreatedAtAction(
                nameof(GetLead),
                new { id = result.Id },
                ApiResponse<LeadDto>.Ok(result, "Lead created."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateLeadDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "User";
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        try
        {
            var result = await _service.UpdateLeadAsync(
                id, dto, userId, userName, role);

            if (result == null)
                return NotFound(ApiResponse<string>.Fail("Lead not found."));

            return Ok(ApiResponse<LeadDto>.Ok(result, "Lead updated."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Administrator";

        var result = await _service.DeleteLeadAsync(
            id, userId, userName);

        if (!result)
            return NotFound(ApiResponse<string>.Fail("Lead not found."));

        return Ok(ApiResponse<string>.Ok("Lead deleted."));
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assign(
        Guid id,
        AssignLeadDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "Administrator";

        try
        {
            var result = await _service.AssignLeadAsync(
                id,
                dto.AssignedEmployeeId,
                userId,
                userName);

            if (result == null)
                return NotFound(ApiResponse<string>.Fail("Lead not found."));

            return Ok(ApiResponse<LeadDto>.Ok(
                result,
                "Lead assignment updated."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpGet("{id:guid}/audit")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Audit(Guid id)
    {
        var result = await _service.GetAuditAsync(id);
        return Ok(ApiResponse<List<LeadAuditDto>>.Ok(result));
    }
}
