// ProjectDtos.cs
namespace SilverlandCRM.Application.DTOs;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProjectDto
{
    public string Name { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateProjectDto : CreateProjectDto { }