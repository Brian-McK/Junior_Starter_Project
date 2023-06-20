using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTO;

public class EmployeeCreateDto
{
    [Required]
    public string? FirstName { get; set; }
    
    [Required]
    public string? LastName { get; set; }
    
    [Required]
    public DateTime? Dob { get; set; }
    
    [Required]
    public string? Email { get; set; }
    
    public string? SkillLevelName { get; set; }
    
    public bool IsActive { get; set; }
    
    public int Age { get; set; }
}