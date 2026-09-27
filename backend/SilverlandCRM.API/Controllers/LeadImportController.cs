using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Infrastructure.Services;

namespace SilverlandCRM.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/leads")]
public class LeadImportController : ControllerBase
{
    private readonly ILeadImportService _service;

    public LeadImportController(ILeadImportService service)
    {
        _service = service;
    }

    [HttpPost("import")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(
                ApiResponse<string>.Fail("Excel file is required."));

        var extension = Path.GetExtension(file.FileName);

        if (!string.Equals(extension, ".xlsx",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                ApiResponse<string>.Fail(
                    "Only .xlsx Excel files are supported."));
        }

        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var userName =
            User.FindFirstValue(ClaimTypes.Name) ?? "Administrator";

        try
        {
            await using var stream = file.OpenReadStream();

            var result = await _service.ImportAsync(
                stream,
                file.FileName,
                userId,
                userName);

            return Ok(
                ApiResponse<ImportSummaryDto>.Ok(
                    result,
                    "Lead import completed."));
        }
        catch (Exception ex)
        {
            return BadRequest(
                ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpGet("import-history")]
    public async Task<IActionResult> History()
    {
        var result = await _service.GetHistoryAsync();

        return Ok(ApiResponse<object>.Ok(result));
    }
}
