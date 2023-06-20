using System.ComponentModel.DataAnnotations;

namespace API.DTO;

public class SkillLevelCreateDto
{
    [Required]
    public string? Name { get; set; }
    
    [Required]
    public string? Description { get; set; }
}