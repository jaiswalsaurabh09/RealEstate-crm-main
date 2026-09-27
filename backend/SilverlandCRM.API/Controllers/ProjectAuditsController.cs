using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Infrastructure.Data;

namespace SilverlandCRM.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/projects")]
public class ProjectAuditsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ProjectAuditsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<IActionResult> GetAudit(Guid id)
    {
        var result = await _db.ProjectAudits
            .AsNoTracking()
            .Where(x => x.ProjectId == id)
            .OrderByDescending(x => x.ChangedAt)
            .ToListAsync();

        return Ok(ApiResponse<object>.Ok(result));
    }
}
