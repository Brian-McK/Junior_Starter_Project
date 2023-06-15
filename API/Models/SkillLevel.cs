using System.ComponentModel.DataAnnotations;

namespace API.Models;

public class SkillLevel
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string? Name { get; set; }
    
    [Required]
    public string? Description { get; set; }
}