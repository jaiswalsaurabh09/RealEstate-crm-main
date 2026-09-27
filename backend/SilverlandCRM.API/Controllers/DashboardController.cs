// DashboardController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Infrastructure.Services;

namespace SilverlandCRM.API.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var role = User.FindFirstValue(ClaimTypes.Role)!;

        var stats = await _dashboardService.GetDashboardStatsAsync(userId, role);
        return Ok(ApiResponse<DashboardStatsDto>.Ok(stats));
    }
}