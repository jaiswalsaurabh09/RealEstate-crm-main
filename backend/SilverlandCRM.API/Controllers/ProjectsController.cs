using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SilverlandCRM.Application.DTOs;
using SilverlandCRM.Infrastructure.Services;

namespace SilverlandCRM.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProjects(
        [FromQuery] bool includeInactive = false)
    {
        var result = await _projectService.GetProjectsAsync(includeInactive);
        return Ok(ApiResponse<List<ProjectDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);

        if (project == null)
            return NotFound(ApiResponse<string>.Fail("Project not found."));

        return Ok(ApiResponse<ProjectDto>.Ok(project));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject(
        [FromBody] CreateProjectDto dto)
    {
        var adminId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var adminName =
            User.FindFirstValue(ClaimTypes.Name) ?? "Administrator";

        try
        {
            var project = await _projectService.CreateProjectAsync(
                dto, adminId, adminName);

            return CreatedAtAction(
                nameof(GetProject),
                new { id = project.Id },
                ApiResponse<ProjectDto>.Ok(project, "Project created."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProject(
        Guid id,
        [FromBody] UpdateProjectDto dto)
    {
        var adminId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var adminName =
            User.FindFirstValue(ClaimTypes.Name) ?? "Administrator";

        try
        {
            var project = await _projectService.UpdateProjectAsync(
                id, dto, adminId, adminName);

            if (project == null)
                return NotFound(ApiResponse<string>.Fail("Project not found."));

            return Ok(ApiResponse<ProjectDto>.Ok(
                project, "Project updated."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var adminId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var adminName =
            User.FindFirstValue(ClaimTypes.Name) ?? "Administrator";

        try
        {
            var result = await _projectService.DeleteProjectAsync(
                id, adminId, adminName);

            if (!result)
                return NotFound(ApiResponse<string>.Fail("Project not found."));

            return Ok(ApiResponse<string>.Ok(
                "Project removed successfully."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> ActivateProject(Guid id)
    {
        var result = await SetActive(id, true);

        return result;
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateProject(Guid id)
    {
        var result = await SetActive(id, false);

        return result;
    }

    private async Task<IActionResult> SetActive(Guid id, bool active)
    {
        var adminId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var adminName =
            User.FindFirstValue(ClaimTypes.Name) ?? "Administrator";

        var result = await _projectService.SetActiveAsync(
            id, active, adminId, adminName);

        if (!result)
            return NotFound(ApiResponse<string>.Fail("Project not found."));

        return Ok(ApiResponse<string>.Ok(
            active ? "Project activated." : "Project deactivated."));
    }
}
