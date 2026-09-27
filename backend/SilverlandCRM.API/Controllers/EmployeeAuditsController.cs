using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Infrastructure.Data;

namespace SilverlandCRM.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/employees")]
public class EmployeeAuditsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EmployeeAuditsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<IActionResult> GetAudit(Guid id)
    {
        var result = await _db.EmployeeAudits
            .AsNoTracking()
            .Where(x => x.EmployeeId == id)
            .OrderByDescending(x => x.ChangedAt)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(result));
    }
}
